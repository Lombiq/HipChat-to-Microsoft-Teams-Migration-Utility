using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lombiq.HipChatToTeams.Models;
using Lombiq.HipChatToTeams.Models.Teams;
using Newtonsoft.Json;
using RestEase;

namespace Lombiq.HipChatToTeams
{
    [Header("User-Agent", "RestEase")]
    public interface ITeamsGraphApi
    {
        [Get("v1.0/me/joinedTeams")]
        Task<GraphApiResult<Team>> GetMyTeamsAsync();

        [Get("v1.0/teams/{teamId}/channels")]
        Task<GraphApiResult<Channel>> GetChannels([Path] string teamId);

        [Post("v1.0/teams/{teamId}/channels")]
        Task<Channel> CreateChannel([Path] string teamId, [Body] Channel channel);

        [Post("beta/teams/{teamId}/channels/{channelId}/chatThreads")]
        Task <ChatThread> CreateThread([Path] string teamId, [Path] string channelId, [Body] ChatThread thread);

        [Post("v1.0/$batch")]
        Task Batch([Body] BatchRequest batchRequest);
    }


    public static class TeamsGraphApiExtensions
    {
        public static Task BatchCreateThreads(this ITeamsGraphApi graphApi, string teamId, string channelId, IEnumerable<ChatThread> threads)
        {
            var url = $"beta/teams/{teamId}/channels/{channelId}/chatThreads";
            var headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" };

            var request = new BatchRequest
            {
                Requests = threads
                    .Select((thread, index) => new Request
                    {
                        Id = index.ToString(),
                        Method = "POST",
                        Url = url,
                        Body = thread,
                        Headers = headers
                    })
            };

            return graphApi.Batch(request);
        }
    }
}
