using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Lombiq.HipChatToTeams.Models.Teams
{
    public class GraphApiResult<T>
    {
        [JsonProperty("value")]
        public IEnumerable<T> Items { get; set; }
    }
}
