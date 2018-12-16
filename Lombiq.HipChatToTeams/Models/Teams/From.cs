using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Lombiq.HipChatToTeams.Models.Teams
{
    public class From
    {
        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("guest")]
        public User Guest { get; set; }
    }
}
