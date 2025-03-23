using System.Security.Cryptography;

namespace FileMonitoringApp.Services.FileHash
{
    internal class Sha1FileHashService : IFileHashService
    {
        public string HashType => "sha-1";

        public async Task<string> ComputeFileHashAsync(string filePath)
        {
            using var sha1 = SHA1.Create();
            using var fileStream = File.OpenRead(filePath);

            byte[] hashBytes = await sha1.ComputeHashAsync(fileStream);

            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}
