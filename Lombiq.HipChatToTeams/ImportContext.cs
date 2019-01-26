using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lombiq.HipChatToTeams
{
    internal class ImportContext
    {
        public Configuration Configuration { get; set; }
        public int MessageBatchSizeOverride { get; set; }
        public bool ShortenNextMessage { get; set; }
        public ITeamsGraphApi GraphApi { get; set; }
        public GraphServiceClient GraphServiceClient { get; set; }
    }
}
