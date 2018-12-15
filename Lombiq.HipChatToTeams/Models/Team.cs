using System;
using System.Collections.Generic;
using System.Text;

namespace Lombiq.HipChatToTeams.Models
{
    public class Team : GraphEntityBase
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool IsArchived { get; set; }
    }
}
