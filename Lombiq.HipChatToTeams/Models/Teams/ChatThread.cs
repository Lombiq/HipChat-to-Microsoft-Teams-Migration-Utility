using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Lombiq.HipChatToTeams.Models.Teams
{
    public class ChatThread : GraphEntityBase
    {
        [JsonProperty("rootMessage")]
        public ChatMessage RootMessage { get; set; }
    }
}
