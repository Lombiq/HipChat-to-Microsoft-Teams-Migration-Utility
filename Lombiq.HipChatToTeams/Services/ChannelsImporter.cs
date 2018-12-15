using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lombiq.HipChatToTeams.Models;
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
                .DeserializeObject<RoomContainer[]>(File.ReadAllText(Path.Combine(configuration.ExportFolderPath, "rooms.json")))
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

                    if (unsupportedChannelNameCharacters.Any(character => roomName.Contains(character)))
                    {
                        roomName = string.Join("", roomName.Split(unsupportedChannelNameCharacters, StringSplitOptions.RemoveEmptyEntries));
                        Console.WriteLine(
                            $"* The \"{room.Name}\" room's name contains at least one character not allowed in channel names " +
                            $"({unsupportedChannelNameCharactersString})). Offending characters were removed: \"{roomName}\".");
                    }

                    await graphApi.CreateChannel(
                        teamToImportInto.Id,
                        new Channel
                        {
                            DisplayName = roomName,
                            Description = room.Topic
                        });
                }
                catch (Exception ex)
                {
                    throw new Exception($"Importing the room \"{room.Name}\" with the description \"{room.Topic}\" failed.", ex);
                }
            }
        }
    }
}
