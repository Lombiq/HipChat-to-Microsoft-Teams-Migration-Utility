using System;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Lombiq.HipChatToTeams.Services;
using Newtonsoft.Json;
using RestEase;

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
                    var configuration = JsonConvert.DeserializeObject<Configuration>(await File.ReadAllTextAsync("AppSettings.json"));

                    var importContext = new ImportContext
                    {
                        // Easier to have full control over the deserialization of AppSettings.json than have it the
                        // ConfigurationBuilder way.
                        Configuration = configuration,
                        GraphApi = RestClient.For<ITeamsGraphApi>(
                            "https://graph.microsoft.com",
                            (request, cancellationToken) =>
                            {
                                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", configuration.AuthorizationToken);
                                return Task.CompletedTask;
                            })
                    };

                    TimestampedConsole.WriteLine("Importing channels from rooms...");
                    await ChannelsImporter.ImportChannelsFromRoomsAsync(importContext);
                    TimestampedConsole.WriteLine("Channels imported.");
                }
                catch (Exception ex)
                {
                    TimestampedConsole.WriteLine("The import failed with the following error: " + ex.ToString());
                }
            }).Wait();

            Console.ReadKey();
        }
    }
}
