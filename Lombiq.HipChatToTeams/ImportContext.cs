using System;
using System.Collections.Generic;
using System.Text;

namespace Lombiq.HipChatToTeams
{
    internal class ImportContext
    {
        public Configuration Configuration { get; set; }
        public int MessageBatchSizeOverride { get; set; }
        public ITeamsGraphApi GraphApi { get; set; }
    }
}
