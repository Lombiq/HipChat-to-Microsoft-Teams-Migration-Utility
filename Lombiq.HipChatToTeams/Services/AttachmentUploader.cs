using Lombiq.HipChatToTeams.Models.Teams;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using IO = System.IO;

namespace Lombiq.HipChatToTeams.Services
{
    internal static class AttachmentUploader
    {
        public static async Task<string> UploadFile(string attachmentPath, Team team, Channel channel, ImportContext importContext, int tryCount = 0)
        {
            var fileSizeBytes = new IO.FileInfo(attachmentPath).Length;
            var fileName = IO.Path.GetFileName(IO.Path.GetDirectoryName(attachmentPath)) + IO.Path.GetExtension(attachmentPath);
            var uploadPath = channel.DisplayName + "/" + fileName;

            var itemRequest = importContext.GraphServiceClient.Groups[team.Id].Drive.Root.ItemWithPath(uploadPath);

            try
            {
                using (var stream = IO.File.OpenRead(attachmentPath))
                {
                    if (fileSizeBytes / 1024 / 1024 > 3.6) // 4MB is the maximum for a simple upload, staying safe with 3.6.
                    {
                        var session = await itemRequest.CreateUploadSession().Request().PostAsync();
                        var maxChunkSize = 320 * 4 * 1024;
                        var provider = new ChunkedUploadProvider(session, importContext.GraphServiceClient, stream, maxChunkSize);

                        var chunckRequests = provider.GetUploadChunkRequests();
                        var exceptions = new List<Exception>();
                        var readBuffer = new byte[maxChunkSize];
                        DriveItem itemResult = null;

                        foreach (var request in chunckRequests)
                        {
                            var result = await provider.GetChunkRequestResponseAsync(request, readBuffer, exceptions);

                            if (result.UploadSucceeded)
                            {
                                itemResult = result.ItemResponse;
                            }
                        }

                        // Retry if upload failed.
                        if (itemResult == null && tryCount < 5)
                        {
                            TimestampedConsole.WriteLine($"Uploading the large attachment \"{attachmentPath}\" failed. Retrying.");
                            return await UploadFile(attachmentPath, team, channel, importContext, ++tryCount);
                        }
                        else
                        {
                            throw new Exception($"The attachment \"{attachmentPath}\" couldn't be uploaded into the \"{channel.DisplayName}\" channel (\"{team.DisplayName}\" team) because upload failed repeatedly. You can try again as this might be just a temporary error. If the issue isn't resolved then delete the file so it's not uploaded.");
                        }
                    }
                    else
                    {
                        await itemRequest.Content.Request().PutAsync<DriveItem>(stream);
                    }

                    return (await itemRequest.Request().GetAsync()).WebUrl;
                }
            }
            catch (ServiceException ex) when (ex.Message.Contains("Unable to provision resource."))
            {
                var waitMinutes = 5;
                TimestampedConsole.WriteLine($"The team's SharePoint site is not yet set up (you can check this under the channel's Files tab) so attachments can't be uploaded. Waiting {waitMinutes} minutes.");
                await Task.Delay(waitMinutes * 60000);
                TimestampedConsole.WriteLine($"Retrying upload.");

                return await UploadFile(attachmentPath, team, channel, importContext, 0);
            }
        }
    }
}
