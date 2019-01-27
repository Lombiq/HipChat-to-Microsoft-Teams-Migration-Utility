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
        public int ShortenLongMessagesToCharacterCount { get; set; }
        public bool UploadAttachments { get; set; }
        public Dictionary<string, string> HipChatRoomsToTeams { get; set; }
        public Dictionary<string, string> HipChatRoomsToChannels { get; set; }
    }
}
