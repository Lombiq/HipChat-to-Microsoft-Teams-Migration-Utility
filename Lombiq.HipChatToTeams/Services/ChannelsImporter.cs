using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lombiq.HipChatToTeams.Models;
using Lombiq.HipChatToTeams.Models.HipChat;
using Lombiq.HipChatToTeams.Models.Teams;
using Newtonsoft.Json;
using RestEase;

namespace Lombiq.HipChatToTeams.Services
{
    internal static class ChannelsImporter
    {
        private static readonly string CursorPath = "ImportCursor.json";


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

            if (!File.Exists(CursorPath))
            {
                await UpdateCursor(new ImportCursor());
            }
            var cursor = JsonConvert.DeserializeObject<ImportCursor>(await File.ReadAllTextAsync(CursorPath));

            var rooms = JsonConvert
                .DeserializeObject<RoomContainer[]>(await File.ReadAllTextAsync(Path.Combine(configuration.ExportFolderPath, "rooms.json")))
                .Select(roomContainer => roomContainer.Room)
                .Skip(cursor.SkipRooms);

            var unsupportedChannelNameCharacters = new[]
            {
                '~', '#', '%', '&', '*', '{', '}', '+', '/', '\\', ':', '<', '>', '?', '|', '\'', '"', '.'
            };
            var unsupportedChannelNameCharactersString = string.Join(", ", unsupportedChannelNameCharacters);

            foreach (var room in rooms)
            {
                try
                {
                    var channelName = room.Name;

                    // This can be used to test a single room again and again, without having to delete anything.
                    //channelName += new Random().Next();
                    //room.Id = 411001;

                    Console.WriteLine("======================");

                    TimestampedConsole.WriteLine($"Starting processing the \"{channelName}\" room.");

                    if (unsupportedChannelNameCharacters.Any(character => channelName.Contains(character)))
                    {
                        channelName = string.Join("", channelName.Split(unsupportedChannelNameCharacters, StringSplitOptions.RemoveEmptyEntries));
                        TimestampedConsole.WriteLine(
                            $"* The \"{room.Name}\" room's name contains at least one character not allowed in channel names " +
                            $"({unsupportedChannelNameCharactersString})). Offending characters were removed: \"{channelName}\".");
                    }

                    Channel channel = (await graphApi.GetChannels(teamToImportInto.Id))
                        .Items
                        .FirstOrDefault(c => c.DisplayName == channelName);

                    if (channel == null)
                    {
                        channel = await graphApi.CreateChannel(
                            teamToImportInto.Id,
                            new Channel
                            {
                                DisplayName = channelName,
                                Description = room.Topic
                            });

                        TimestampedConsole.WriteLine($"Created the \"{channelName}\" channel. Starting importing messages.");
                    }
                    else
                    {
                        TimestampedConsole.WriteLine($"Starting importing messages into the existing \"{channelName}\" channel.");
                    }

                    var roomFolderPath = Path.Combine(configuration.ExportFolderPath, "rooms", room.Id.ToString());
                    var historyFilePath = Path.Combine(roomFolderPath, "history.json");
                    var messages = JsonConvert
                        .DeserializeObject<Message[]>(await File.ReadAllTextAsync(historyFilePath), new MessageJsonConverter())
                        .Where(message => !(message is ArchiveRoomMessage))
                        .Skip(cursor.SkipMessages);

                    foreach (var message in messages)
                    {
                        var chatMessage = new ChatMessage
                        {
                            Body = new ItemBody
                            {
                                Content = message.Body,
                                ContentType = "1"
                            },
                            CreatedDateTime = message.Timestamp
                        };

                        if (message is UserMessage userMessage)
                        {
                            // Users are not fetched yet and this doesn't work, so using a hack to show
                            // the user's name in the message body for now.
                            chatMessage.From = new IdentitySet
                            {
                                User = new User
                                {
                                    DisplayName = userMessage.Sender.Name
                                }
                            };

                            chatMessage.Body.Content = $"{userMessage.Sender.Name}:<br>{chatMessage.Body.Content}";

                            if (userMessage.Attachment != null)
                            {
                                var attachmentPathSegments = userMessage.Attachment.Path.Split(new[] { '/' });
                                var attachmentPath = Path.Combine(roomFolderPath, "files", attachmentPathSegments[0], attachmentPathSegments[1]);

                                // The content type is not yet used because attaching files doesn't take effect any
                                // way, and posting bigger files won't work either.
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
                                        ContentUrl = userMessage.Attachment.Url,
                                        ContentType = "reference"
                                    }
                                };
                            }
                        }
                        else
                        {
                            // This doesn't work.
                            //chatMessage.From = new From
                            //{
                            //    Guest = new User
                            //    {
                            //        DisplayName = ((NotificationMessage)message).Sender
                            //    }
                            //};

                            chatMessage.Body.Content = $"{((NotificationMessage)message).Sender}:<br>{chatMessage.Body.Content}";
                        }

                        await graphApi.CreateThread(
                            teamToImportInto.Id,
                            channel.Id,
                            new ChatThread
                            {
                                RootMessage = chatMessage
                            });

                        cursor.SkipMessages++;
                        await UpdateCursor(cursor);

                        if (cursor.SkipMessages % 50 == 0)
                        {
                            TimestampedConsole.WriteLine($"{cursor.SkipMessages} messages imported into the channel.");
                        }
                    }

                    cursor.SkipRooms++;
                    cursor.SkipMessages = 0;
                    await UpdateCursor(cursor);

                    TimestampedConsole.WriteLine($"Messages imported into the \"{channel.DisplayName}\" channel.");

                    Console.WriteLine("======================");
                }
                catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    TimestampedConsole.WriteLine($"API requests are being throttled. Waiting for {configuration.ThrottlingCooldownSeconds} seconds, then retrying. If this happens again and again then try opening the Graph Explorer in a browser and logging in again, and getting a new authorization token. If that doesn't help then close the app, configure a longer cooldown time, then restart (or wait some time before that).");
                    await Task.Delay(configuration.ThrottlingCooldownSeconds * 1000);
                    await ImportChannelsFromRoomsAsync(importContext);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Importing the room \"{room.Name}\" with the description \"{room.Topic}\" failed.", ex);
                }
            }
        }


        private static Task UpdateCursor(ImportCursor cursor) =>
            File.WriteAllTextAsync(CursorPath, JsonConvert.SerializeObject(cursor));
    }
}
