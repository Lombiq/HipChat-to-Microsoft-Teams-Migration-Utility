using Newtonsoft.Json;

namespace Lombiq.HipChatToTeams.Models.Teams
{
    public class ChatMessageAttachment : GraphEntityBase
    {
        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        [JsonProperty("contentUrl")]
        public string ContentUrl { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("thumbnailUrl")]
        public string ThumbnailUrl { get; set; }
    }
}
