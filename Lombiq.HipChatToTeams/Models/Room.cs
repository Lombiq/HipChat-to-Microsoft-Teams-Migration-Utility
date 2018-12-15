﻿using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Lombiq.HipChatToTeams.Models
{
    internal class Room
    {
        [JsonProperty(PropertyName = "created")]
        public DateTime CreatedLocal { get; set; }
        public int Id { get; set; }
        public bool IsArchived { get; set; }
        public string Name { get; set; }
        public string Topic { get; set; }
    }
}