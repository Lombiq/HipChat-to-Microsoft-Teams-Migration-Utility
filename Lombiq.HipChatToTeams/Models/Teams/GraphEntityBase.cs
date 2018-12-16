using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Lombiq.HipChatToTeams.Models.Teams
{
    public class GraphEntityBase
    {
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Id { get; set; }
    }
}
