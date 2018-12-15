using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Graph;

namespace Lombiq.HipChatToTeams
{
    internal class ImportContext
    {
        public Configuration Configuration { get; set; }
        public GraphServiceClient GraphServiceClient { get; set; }
    }
}
