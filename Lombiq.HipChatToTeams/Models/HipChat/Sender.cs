using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Lombiq.HipChatToTeams.Models.HipChat
{
    public class Sender
    {
        public int Id { get; set; }

        [JsonProperty("mention_name")]
        public string MentionName { get; set; }

        public string Name { get; set; }
    }
}
