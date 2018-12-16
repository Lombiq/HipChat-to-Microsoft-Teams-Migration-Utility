using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lombiq.HipChatToTeams.Models;
using Lombiq.HipChatToTeams.Models.HipChat;
using Lombiq.HipChatToTeams.Models.Teams;
using Newtonsoft.Json;

namespace Lombiq.HipChatToTeams.Services
{
    internal static class ChannelsImporter
    {
        public static async Task ImportChannelsFromRoomsAsync(ImportContext importContext)
        {
            var configuration = importContext.Configuration;
            var graphApi = importContext.GraphApi;

            var teams = await graphApi.GetMyTeamsAsync();
            var teamToImportInto = teams.Items.SingleOrDefault(team => team.DisplayName == configuration.TeamNameToImportChannelsInto);

            if (teamToImportInto == null)
            {
                throw new Exception($"The team \"{configuration.TeamNameToImportChannelsInto}\" that was configured to import channels into wasn't found among the teams joined by the user authenticated for the import.");
            }

            var rooms = JsonConvert
                .DeserializeObject<RoomContainer[]>(await File.ReadAllTextAsync(Path.Combine(configuration.ExportFolderPath, "rooms.json")))
                .Select(roomContainer => roomContainer.Room);

            var unsupportedChannelNameCharacters = new[]
            {
                '~', '#', '%', '&', '*', '{', '}', '+', '/', '\\', ':', '<', '>', '?', '|', '\'', '"', '.'
            };
            var unsupportedChannelNameCharactersString = string.Join(", ", unsupportedChannelNameCharacters);

            foreach (var room in rooms)
            {
                try
                {
                    var roomName = room.Name;
                    roomName += new Random().Next();
                    room.Id = 411001;

                    Console.WriteLine("======================");

                    TimestampedConsole.WriteLine($"Starting processing the \"{roomName}\" room.");

                    if (unsupportedChannelNameCharacters.Any(character => roomName.Contains(character)))
                    {
                        roomName = string.Join("", roomName.Split(unsupportedChannelNameCharacters, StringSplitOptions.RemoveEmptyEntries));
                        TimestampedConsole.WriteLine(
                            $"* The \"{room.Name}\" room's name contains at least one character not allowed in channel names " +
                            $"({unsupportedChannelNameCharactersString})). Offending characters were removed: \"{roomName}\".");
                    }

                    var channel = await graphApi.CreateChannel(
                        teamToImportInto.Id,
                        new Channel
                        {
                            DisplayName = roomName,
                            Description = room.Topic
                        });

                    TimestampedConsole.WriteLine($"Created the \"{channel.DisplayName}\" channel. Starting importing messages.");

                    var roomFolderPath = Path.Combine(configuration.ExportFolderPath, "rooms", room.Id.ToString());
                    var historyFilePath = Path.Combine(roomFolderPath, "history.json");
                    var messages = JsonConvert
                        .DeserializeObject<Message[]>(await File.ReadAllTextAsync(historyFilePath), new MessageJsonConverter())
                        .Where(message => !(message is ArchiveRoomMessage));

                    foreach (var message in messages)
                    {
                        var chatMessage = new ChatMessage
                        {
                            Body = new ItemBody
                            {
                                Content = message.Body,
                                ContentType = (message is NotificationMessage) ? "1" : "0"
                            },
                            CreatedDateTime = message.Timestamp
                        };

                        if (message is UserMessage userMessage && userMessage.Attachment != null)
                        {
                            var attachmentPathSegments = userMessage.Attachment.Path.Split(new[] { '/' });
                            var attachmentPath = Path.Combine(roomFolderPath, "files", attachmentPathSegments[0], attachmentPathSegments[1]);
                            
                            var contentType = MimeTypeMap.List.MimeTypeMap
                                .GetMimeType(Path.GetExtension(attachmentPath))
                                .FirstOrDefault();

                            if (contentType == null || 
                                    (!contentType.StartsWith("image/") && 
                                    !contentType.StartsWith("video/") && 
                                    !contentType.StartsWith("audio/") && 
                                    !contentType.StartsWith("application/vnd.microsoft.card.")))
                            {
                                contentType = "file";
                            }

                            chatMessage.Attachments = new[]
                            {
                                new ChatMessageAttachment
                                {
                                    ContentUrl = $"data:{contentType};base64," + Convert.ToBase64String(await File.ReadAllBytesAsync(attachmentPath)),
                                    ContentType = contentType
                                }
                            };
                        }

                        await graphApi.CreateThread(
                            teamToImportInto.Id,
                            channel.Id,
                            new ChatThread
                            {
                                RootMessage = chatMessage
                            });
                    }

                    TimestampedConsole.WriteLine($"Messages imported into the \"{channel.DisplayName}\" channel.");

                    Console.WriteLine("======================");
                }
                catch (Exception ex)
                {
                    throw new Exception($"Importing the room \"{room.Name}\" with the description \"{room.Topic}\" failed.", ex);
                }
            }
        }
    }
}
