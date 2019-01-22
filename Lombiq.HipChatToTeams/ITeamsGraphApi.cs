using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lombiq.HipChatToTeams.Models;
using Lombiq.HipChatToTeams.Models.Teams;
using RestEase;

namespace Lombiq.HipChatToTeams
{
    [Header("User-Agent", "RestEase")]
    public interface ITeamsGraphApi
    {
        [Get("v1.0/me/joinedTeams")]
        Task<GraphApiResult<Team>> GetMyTeamsAsync();

        [Get("v1.0/teams/{teamId}/channels")]
        Task<GraphApiResult<Channel>> GetChannelsAsync([Path] string teamId);

        [Post("v1.0/teams/{teamId}/channels")]
        Task<Channel> CreateChannelAsync([Path] string teamId, [Body] Channel channel);

        [Post("beta/teams/{teamId}/channels/{channelId}/chatThreads")]
        Task <ChatThread> CreateThreadAsync([Path] string teamId, [Path] string channelId, [Body] ChatThread thread);
    }
}
