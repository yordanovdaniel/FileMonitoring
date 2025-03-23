using System.Security.Cryptography;

namespace FileMonitoringApp.Services.FileHash
{
    internal class Sha256FileHashService : IFileHashService
    {
        public string HashType => "sha-256";

        public async Task<string> ComputeFileHashAsync(string filePath)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var fileStream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = await sha256.ComputeHashAsync(fileStream);

                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                }
            }
        }
    }
}
