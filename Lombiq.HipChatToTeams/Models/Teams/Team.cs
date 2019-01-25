using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Lombiq.HipChatToTeams.Models.Teams
{
    public class Team : GraphEntityBase
    {
        [JsonProperty("template@odata.bind")]
        public string TemplateBind { get; set; } = "https://graph.microsoft.com/beta/teamsTemplates/standard";

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("isArchived", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool IsArchived { get; set; }

        [JsonProperty("visibility")]
        public TeamVisibilityType Visibility { get; set; } = TeamVisibilityType.Private;
    }


    [JsonConverter(typeof(StringEnumConverter))]
    public enum TeamVisibilityType
    {
        [EnumMember(Value = "private")]
        Private,

        [EnumMember(Value = "public")]
        Public
    }
}
