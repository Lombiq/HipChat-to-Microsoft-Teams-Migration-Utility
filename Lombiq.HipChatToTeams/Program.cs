using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Lombiq.HipChatToTeams.Services;
using RestEase;

namespace Lombiq.HipChatToTeams
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                var configuration = new Configuration
                {
                    ExportFolderPath = @"E:\export",
                    // If channels could be moved (https://microsoftteams.uservoice.com/forums/555103-public/suggestions/16939708-move-channels-into-other-teams)
                    // this would be enough.
                    TeamNameToImportChannelsInto = "Import test",
                    AuthorizationToken = ""
                };


                var importContext = new ImportContext
                {
                    Configuration = configuration,
                    GraphApi = RestClient.For<ITeamsGraphApi>(
                        "https://graph.microsoft.com",
                        async (request, cancellationToken) =>
                        {
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", configuration.AuthorizationToken);
                        })
                };

                Console.WriteLine("======================");
                Console.WriteLine("Importing channels from rooms...");
                await ChannelsImporter.ImportChannelsFromRoomsAsync(importContext);
                Console.WriteLine("Channels imported.");
                Console.WriteLine("======================");
            }).Wait();

            Console.ReadKey();
        }
    }
}
