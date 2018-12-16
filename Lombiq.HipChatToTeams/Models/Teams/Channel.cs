using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Lombiq.HipChatToTeams.Models.Teams
{
    public class Channel : GraphEntityBase
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
