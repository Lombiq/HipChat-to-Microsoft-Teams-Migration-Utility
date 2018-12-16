using System;
using Lombiq.HipChatToTeams.Models;
using Lombiq.HipChatToTeams.Models.HipChat;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lombiq.HipChatToTeams.Services
{
    internal class MessageJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(Message));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var messageContainer = JObject.Load(reader);
            var messageType = ((JProperty)messageContainer.First).Name;
            var message = messageContainer.First.First;

            // The timestamp contains microseconds after a "Z", which breaks deserialization.
            message["timestamp"] = message["timestamp"].ToString().Split("Z")[0];

            if (messageType == "NotificationMessage")
            {
                return message.ToObject<NotificationMessage>();
            }
            else if (messageType == "UserMessage")
            {
                return message.ToObject<UserMessage>();
            }
            else if (messageType == "ArchiveRoomMessage")
            {
                return message.ToObject<ArchiveRoomMessage>();
            }
            else if (messageType == "TopicRoomMessage")
            {
                return message.ToObject<TopicRoomMessage>();
            }
            else
            {
                throw new NotSupportedException($"The HipChat message type \"{messageType}\" is not supported.");
            }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
