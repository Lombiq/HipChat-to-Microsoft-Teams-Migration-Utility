using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Converters;

namespace Lombiq.HipChatToTeams.Services
{
    public class GraphApiDateTimeConverter : IsoDateTimeConverter
    {
        public GraphApiDateTimeConverter()
        {
            base.DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        }
    }
}
