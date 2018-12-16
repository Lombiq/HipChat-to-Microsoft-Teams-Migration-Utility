using System;
using System.Collections.Generic;
using System.Text;

namespace Lombiq.HipChatToTeams.Models
{
    public class ImportCursor
    {
        public int SkipRooms { get; set; }
        public int SkipMessages { get; set; }
    }
}
