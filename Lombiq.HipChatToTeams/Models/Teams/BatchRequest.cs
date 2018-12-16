using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Lombiq.HipChatToTeams.Models.Teams
{
    public class BatchRequest
    {
        [JsonProperty("requests")]
        public IEnumerable<Request> Requests { get; set; }
    }
}
