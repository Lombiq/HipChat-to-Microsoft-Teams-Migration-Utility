using System;
using System.Collections.Generic;
using System.Text;

namespace Lombiq.HipChatToTeams
{
    internal static class TimestampedConsole
    {
        public static void WriteLine(string value) => Console.WriteLine(DateTime.Now + " " + value);
    }
}
