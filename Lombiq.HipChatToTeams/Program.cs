using Lombiq.HipChatToTeams.Services;
using Microsoft.Graph;
using Newtonsoft.Json;
using RestEase;
using System;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Lombiq.HipChatToTeams
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                try
                {
                    var configuration = JsonConvert.DeserializeObject<Configuration>(await System.IO.File.ReadAllTextAsync("AppSettings.json"));

                    if (string.IsNullOrEmpty(configuration.AuthorizationToken))
                    {
                        throw new Exception("You need to supply an authorization token in the AppSettings.json file, see the documentation.");
                    }

                    var importContext = new ImportContext
                    {
                        // Easier to have full control over the deserialization of AppSettings.json than have it the
                        // ConfigurationBuilder way.
                        Configuration = configuration,
                        MessageBatchSizeOverride = configuration.NumberOfHipChatMessagesToImportIntoTeamsMessage,
                        GraphApi = RestClient.For<ITeamsGraphApi>(
                            "https://graph.microsoft.com",
                            (request, cancellationToken) =>
                            {
                                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", configuration.AuthorizationToken);
                                return Task.CompletedTask;
                            }),
                        GraphServiceClient = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) =>
                        {
                            requestMessage
                                .Headers
                                .Authorization = new AuthenticationHeaderValue("bearer", configuration.AuthorizationToken);

                            return Task.FromResult(0);
                        }))
                    };

                    TimestampedConsole.WriteLine("Importing channels from rooms...");
                    await ChannelsImporter.ImportChannelsFromRoomsAsync(importContext);
                    TimestampedConsole.WriteLine("Channels imported.");
                }
                catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    TimestampedConsole.WriteLine("Authorizing with the Graph API failed. The authorization token configured may be expired or doesn't have all the required permissions.");
                }
                catch (Exception ex)
                {
                    TimestampedConsole.WriteLine("The import failed with the following unrecoverable error: " + Environment.NewLine + ex.ToString());
                }
            }).Wait();

            Console.ReadKey();
        }
    }
}
