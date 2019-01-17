using System;
using System.Collections.Generic;
using System.Text;

namespace Lombiq.HipChatToTeams
{
    internal class Configuration
    {
        public string ExportFolderPath { get; set; }
        public string AuthorizationToken { get; set; }
        public int NumberOfHipChatMessagesToImportIntoTeamsMessage { get; set; }
        public Dictionary<string, string> HipChatRoomsToTeams { get; set; }
    }
}
