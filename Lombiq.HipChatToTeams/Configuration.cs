using System;
using System.Collections.Generic;
using System.Text;

namespace Lombiq.HipChatToTeams
{
    internal class Configuration
    {
        public string ExportFolderPath { get; set; }
        public string AuthorizationToken { get; set; }
        public string TeamNameToImportChannelsInto { get; set; }
    }
}
