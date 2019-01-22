using System;
using System.Collections.Generic;
using System.Text;

namespace Lombiq.HipChatToTeams.Models.Teams
{
    // See: https://docs.microsoft.com/en-us/graph/api/resources/teamsasyncoperation?view=graph-rest-beta
    public class TeamsAsyncOperation : GraphEntityBase
    {
        public string OperationType { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string Status { get; set; }
        public DateTime LastActionDateTime { get; set; }
        public int AttemptsCount { get; set; }
        public string TargetResourceId { get; set; }
        public string TargetResourceLocation { get; set; }
        public string Error { get; set; }
    }
}
