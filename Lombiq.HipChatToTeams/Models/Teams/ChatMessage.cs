using System;
using System.Collections.Generic;
using System.Text;
using Lombiq.HipChatToTeams.Services;
using Newtonsoft.Json;

namespace Lombiq.HipChatToTeams.Models.Teams
{
    public class ChatMessage : GraphEntityBase
    {
        [JsonProperty("from")]
        public IdentitySet From { get; set; }

        [JsonProperty("createdDateTime")]
        [JsonConverter(typeof(GraphApiDateTimeConverter))]
        public DateTime CreatedDateTime { get; set; }

        [JsonProperty("body")]
        public ItemBody Body { get; set; }

        [JsonProperty("attachments")]
        public IEnumerable<ChatMessageAttachment> Attachments { get; set; }
    }
}
