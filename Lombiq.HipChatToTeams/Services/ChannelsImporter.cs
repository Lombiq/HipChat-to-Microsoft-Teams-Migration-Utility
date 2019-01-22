using System;
using System.IO;
using System.Linq;
using System.Net;
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
        private const string CursorPath = "ImportCursor.json";

        private const int DefaultThrottlingCooldownMinutes = 10;
        private static int _throttlingCooldownMinutes = DefaultThrottlingCooldownMinutes;


        public static async Task ImportChannelsFromRoomsAsync(ImportContext importContext)
        {
            var configuration = importContext.Configuration;
            var graphApi = importContext.GraphApi;

            var teams = await graphApi.GetMyTeamsAsync();

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
                if (!configuration.HipChatRoomsToTeams.TryGetValue(room.Name, out string teamNameToImportChannelInto))
                {
                    teamNameToImportChannelInto = configuration.HipChatRoomsToTeams["$Default"];
                }

                var teamToImportInto = teams.Items.SingleOrDefault(team => team.DisplayName == teamNameToImportChannelInto);

                if (teamToImportInto == null)
                {
                    TimestampedConsole.WriteLine($"The team \"{teamNameToImportChannelInto}\" doesn't exist, so attempting to create it.");

                    using (var response = await graphApi.CreateTeamAsync(new Team { DisplayName = teamNameToImportChannelInto }))
                    {
                        if (!response.Headers.TryGetValues("Location", out var locations) || locations.Count() != 1)
                        {
                            throw new Exception($"Attempted to create the \"{teamNameToImportChannelInto}\" team but the operation didn't return correctly. Try to create the team manually.");
                        }

                        var location = locations.Single();
                        var operation = await graphApi.GetAsyncOperation(location);

                        var tries = 0;
                        while (operation.Status != "succeeded")
                        {
                            if (tries > 10)
                            {
                                throw new Exception($"Attempted to create the \"{teamNameToImportChannelInto}\" team but the operation didn't succeed after plenty of time. Try to create the team manually.");
                            }

                            // https://docs.microsoft.com/en-us/graph/api/resources/teamsasyncoperation?view=graph-rest-beta
                            // says to wait >30s.
                            await Task.Delay(31000);

                            operation = await graphApi.GetAsyncOperation(location);

                            tries++;
                        }

                        teamToImportInto = new Team
                        {
                            Id = operation.TargetResourceId,
                            DisplayName = teamNameToImportChannelInto
                        };
                    }

                    TimestampedConsole.WriteLine($"Created the \"{teamNameToImportChannelInto}\" team.");
                }

                try
                {
                    if (!configuration.HipChatRoomsToChannels.TryGetValue(room.Name, out string channelName))
                    {
                        channelName = room.Name;
                    }

                    // This can be used to test a single room again and again, without having to delete anything.
                    //channelName += new Random().Next();
                    //room.Id = 411001;

                    Console.WriteLine("======================");

                    TimestampedConsole.WriteLine($"Starting processing the \"{room.Name}\" room.");

                    if (unsupportedChannelNameCharacters.Any(character => channelName.Contains(character)))
                    {
                        channelName = string.Join("", channelName.Split(unsupportedChannelNameCharacters, StringSplitOptions.RemoveEmptyEntries));
                        TimestampedConsole.WriteLine(
                            $"* The \"{room.Name}\" room's name contains at least one character not allowed in channel names " +
                            $"({unsupportedChannelNameCharactersString})). Offending characters were removed: \"{channelName}\".");
                    }

                    Channel channel = (await graphApi.GetChannelsAsync(teamToImportInto.Id))
                        .Items
                        .FirstOrDefault(c => c.DisplayName == channelName);

                    if (channel == null)
                    {
                        channel = await graphApi.CreateChannelAsync(
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
                        .Where(message => message != null && !(message is ArchiveRoomMessage))
                        // To preserve the original order at least we need to reverse the data set. This wouldn't be
                        // needed if the created message's timestamp would actually take effect, see:
                        // https://github.com/Lombiq/HipChat-to-Microsoft-Teams-Migration-Utility/issues/1
                        .Reverse()
                        .Skip(cursor.SkipMessages);

                    var batchSize = configuration.NumberOfHipChatMessagesToImportIntoTeamsMessage;

                    if (importContext.MessageBatchSizeOverride > 0)
                    {
                        batchSize = importContext.MessageBatchSizeOverride;
                    }

                    while (messages.Any())
                    {
                        var messageBatch = messages.Take(batchSize);

                        var chatMessage = new ChatMessage
                        {
                            Body = new ItemBody
                            {
                                ContentType = "1"
                            },
                            CreatedDateTime = messageBatch.First().Timestamp
                        };

                        var batchedMessageBody = string.Empty;

                        foreach (var message in messageBatch)
                        {
                            var messageBody = message.Body;

                            if (importContext.ShortenNextMessage)
                            {
                                messageBody = 
                                    messageBody.Substring(0, configuration.ShortenLongMessagesToCharacterCount) + "...";
                            }

                            if (message is UserMessage userMessage)
                            {
                                // Users are not fetched yet and this doesn't work, so using a hack to show
                                // the user's name in the message body for now.
                                //chatMessage.From = new IdentitySet
                                //{
                                //    User = new User
                                //    {
                                //        DisplayName = userMessage.Sender.Name
                                //    }
                                //};

                                if (messageBody.StartsWith("/code "))
                                {
                                    messageBody = $"<pre><code>{messageBody.Substring(6)}</code></pre>";
                                }

                                if (messageBody.StartsWith("/quote "))
                                {
                                    messageBody = $"<blockquote>{messageBody.Substring(7)}</blockquote>";
                                }

                                messageBody = $"{userMessage.Sender.Name}:<br>{messageBody}";

                                // Attachments don't work, see: https://github.com/Lombiq/HipChat-to-Microsoft-Teams-Migration-Utility/issues/2
                                //if (userMessage.Attachment != null)
                                //{
                                //    var attachmentPathSegments = userMessage.Attachment.Path.Split(new[] { '/' });
                                //    var attachmentPath = Path.Combine(roomFolderPath, "files", attachmentPathSegments[0], attachmentPathSegments[1]);

                                //    // The content type is not yet used because attaching files doesn't take effect any
                                //    // way, and posting bigger files won't work either.
                                //    var contentType = MimeTypeMap.List.MimeTypeMap
                                //        .GetMimeType(Path.GetExtension(attachmentPath))
                                //        .FirstOrDefault();

                                //    if (contentType == null ||
                                //            (!contentType.StartsWith("image/") &&
                                //            !contentType.StartsWith("video/") &&
                                //            !contentType.StartsWith("audio/") &&
                                //            !contentType.StartsWith("application/vnd.microsoft.card.")))
                                //    {
                                //        contentType = "file";
                                //    }

                                //    chatMessage.Attachments = new[]
                                //    {
                                //        new ChatMessageAttachment
                                //        {
                                //            // This could work, but doesn't work either:
                                //            //ContentUrl = $"data:{contentType};base64," + Convert.ToBase64String(await File.ReadAllBytesAsync(attachmentPath)),
                                //            //ContentType = contentType
                                //            ContentUrl = userMessage.Attachment.Url,
                                //            ContentType = "reference"
                                //        }
                                //    };
                                //}
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

                                messageBody = $"{((NotificationMessage)message).Sender}:<br>{messageBody}";
                            }

                            messageBody = message.Timestamp.ToString() + " " + messageBody;
                            if (batchSize != 1) messageBody += "<hr>";

                            batchedMessageBody += messageBody;
                            importContext.ShortenNextMessage = false;
                        }

                        chatMessage.Body.Content = batchedMessageBody;

                        await graphApi.CreateThreadAsync(
                            teamToImportInto.Id,
                            channel.Id,
                            new ChatThread
                            {
                                RootMessage = chatMessage
                            });

                        cursor.SkipMessages += batchSize;
                        await UpdateCursor(cursor);
                        _throttlingCooldownMinutes = DefaultThrottlingCooldownMinutes;

                        // Waiting a bit to avoid API throttling.
                        await Task.Delay(500);

                        if (cursor.SkipMessages % 50 == 0)
                        {
                            TimestampedConsole.WriteLine($"{cursor.SkipMessages} messages imported into the channel.");
                        }

                        messages = messages.Skip(batchSize);

                        batchSize = configuration.NumberOfHipChatMessagesToImportIntoTeamsMessage;
                        importContext.MessageBatchSizeOverride = batchSize;
                    }

                    cursor.SkipRooms++;
                    cursor.SkipMessages = 0;
                    await UpdateCursor(cursor);

                    TimestampedConsole.WriteLine($"Messages imported into the \"{channel.DisplayName}\" channel.");

                    Console.WriteLine("======================");
                }
                catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    TimestampedConsole.WriteLine($"API requests are being throttled. Waiting for {_throttlingCooldownMinutes} minutes, then retrying. If this happens again and again then close the app and wait some time (more than an hour, or sometimes even a day) before starting it again.");

                    // While some APIs return a Retry-After header to indicate when you should retry a throttled
                    // request (see: https://docs.microsoft.com/en-us/graph/throttling) the Teams endpoints don't.
                    // Also, all endpoints seem to have their own limits, because after the message creation is
                    // throttled e.g. the user APIs still work. So we need to use such hacks.
                    await Task.Delay(_throttlingCooldownMinutes * 60000);
                    _throttlingCooldownMinutes *= 2;

                    await ImportChannelsFromRoomsAsync(importContext);
                }
                catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    var waitSeconds = 10;
                    TimestampedConsole.WriteLine($"A request failed with the error Service Unavailable. Waiting {waitSeconds}s, then retrying.");
                    await Task.Delay(waitSeconds * 1000);

                    await ImportChannelsFromRoomsAsync(importContext);
                }
                catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.RequestEntityTooLarge)
                {
                    if (importContext.MessageBatchSizeOverride == configuration.NumberOfHipChatMessagesToImportIntoTeamsMessage &&
                        importContext.MessageBatchSizeOverride > 5)
                    {
                        importContext.MessageBatchSizeOverride = 5;
                    }
                    else if (importContext.MessageBatchSizeOverride != 1)
                    {
                        importContext.MessageBatchSizeOverride = 1;
                    }
                    else
                    {
                        if (configuration.ShortenLongMessagesToCharacterCount > 0)
                        {
                            importContext.ShortenNextMessage = true;

                            await ImportChannelsFromRoomsAsync(importContext);
                            return;
                        }
                        else
                        {
                            throw new Exception($"The next message to import from the \"{room.Name}\" room is too large. You need to manually shorten it in the corresponding JSON file in the HipChat export package.");
                        }
                    }

                    TimestampedConsole.WriteLine($"Importing {configuration.NumberOfHipChatMessagesToImportIntoTeamsMessage} HipChat messages into a Teams message resulted in a message too large. Retrying with just {importContext.MessageBatchSizeOverride} messages.");

                    await ImportChannelsFromRoomsAsync(importContext);
                }
                catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Importing the room \"{room.Name}\" with the description \"{room.Topic}\" failed. This error can't be retried.", ex);
                }
            }
        }


        private static Task UpdateCursor(ImportCursor cursor) =>
            File.WriteAllTextAsync(CursorPath, JsonConvert.SerializeObject(cursor));
    }
}
