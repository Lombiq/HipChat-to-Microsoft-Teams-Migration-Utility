using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Lombiq.HipChatToTeams.Models.Teams
{
    public class ChatMessage : GraphEntityBase
    {
        // from

        [JsonProperty("createdDateTime")]
        public DateTime CreatedDateTime { get; set; }

        [JsonProperty("body")]
        public ItemBody Body { get; set; }

        [JsonProperty("attachments")]
        public IEnumerable<ChatMessageAttachment> Attachments { get; set; }
    }
}
