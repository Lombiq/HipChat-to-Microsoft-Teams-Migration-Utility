using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Lombiq.HipChatToTeams.Models.Teams
{
    public class ItemBody
    {
        [JsonProperty("contentType")]
        // 0 = plain text, 1 = HTML.
        public string ContentType { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }
}
