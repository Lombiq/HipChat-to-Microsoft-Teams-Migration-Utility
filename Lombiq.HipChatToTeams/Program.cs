using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using OpenSSL.Crypto;

namespace Lombiq.HipChatToTeams
{
    class Program
    {
        static void Main(string[] args)
        {
            var hipChatExportFilePath = @"E:\hipchat-72182-2018-12-15_13-12-45.tar.gz.aes";
            var hipChatExportFilePassword = "Lombiq Exodus";

            using (var cipherContext = new CipherContext(Cipher.AES_256_CBC))
            using (var exportFile = MemoryMappedFile.CreateFromFile(hipChatExportFilePath))
            using (var fileStream = exportFile.CreateViewStream())
            using (var memoryStream = new HugeMemoryStream())
            {
                fileStream.CopyTo(memoryStream);

                var decryptedBytes = cipherContext
                    .Decrypt(
                        memoryStream.ToArray(),
                        Encoding.ASCII.GetBytes(hipChatExportFilePassword),
                        Encoding.ASCII.GetBytes(""));

                File.WriteAllBytes(@"E:\export.tar.gz", decryptedBytes);
            }
        }
    }
}
