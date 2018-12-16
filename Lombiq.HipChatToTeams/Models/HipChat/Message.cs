using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Lombiq.HipChatToTeams.Models.HipChat
{
    public class Message
    {
        public DateTime Timestamp { get; set; }
        
        [JsonProperty("message")]
        public string Body { get; set; }
    }


    public class UserMessage : Message
    {
        public Sender Sender { get; set; }
        public Attachment Attachment { get; set; }
    }


    public class NotificationMessage : Message
    {
        public string Sender { get; set; }
    }

    public class ArchiveRoomMessage : UserMessage
    {
    }

    public class TopicRoomMessage : UserMessage
    {
    }
}
