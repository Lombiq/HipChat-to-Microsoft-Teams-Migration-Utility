using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Lombiq.HipChatToTeams.Services;
using Microsoft.Graph;

namespace Lombiq.HipChatToTeams
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new Configuration
            {
                ExportFolderPath = @"E:\export",
                AuthorizationToken = "",
                TeamNameToImportChannelsInto = "Import test"
            };


            var graphServiceClient = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) => {
                requestMessage
                    .Headers
                    .Authorization = new AuthenticationHeaderValue("bearer", configuration.AuthorizationToken);

                return Task.FromResult(0);
            }));

            var importContext = new ImportContext
            {
                Configuration = configuration,
                GraphServiceClient = graphServiceClient
            };

            Console.WriteLine("Importing channels from rooms...");
            ChannelsImporter.ImportChannelsFromRooms(importContext);
            Console.WriteLine("Channels imported.");
        }
    }
}
