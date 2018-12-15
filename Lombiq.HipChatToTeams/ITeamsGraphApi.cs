using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lombiq.HipChatToTeams.Models;
using RestEase;

namespace Lombiq.HipChatToTeams
{
    public interface ITeamsGraphApi
    {
        [Get("v1.0/me/joinedTeams")]
        Task<GraphApiResult<Team>> GetMyTeamsAsync();

        [Post("v1.0/teams/{teamId}/channels")]
        Task CreateChannel([Path] string teamId, [Body] Channel channel);
    }
}
