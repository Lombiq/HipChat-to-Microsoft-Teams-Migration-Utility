using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Lombiq.HipChatToTeams.Models;
using Newtonsoft.Json;

namespace Lombiq.HipChatToTeams.Services
{
    internal static class ChannelsImporter
    {
        public static void ImportChannelsFromRooms(ImportContext importContext)
        {
            var rooms = JsonConvert
                .DeserializeObject<RoomContainer[]>(File.ReadAllText(Path.Combine(importContext.Configuration.ExportFolderPath, "rooms.json")))
                .Select(roomContainer => roomContainer.Room);

            var teams = importContext.GraphServiceClient.Me.

            System.Diagnostics.Debugger.Break();
        }
    }
}
