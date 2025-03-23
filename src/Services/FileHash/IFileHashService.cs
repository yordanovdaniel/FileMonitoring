namespace FileMonitoringApp.Services.FileHash
{
    internal interface IFileHashService
    {
        public string HashType { get; }

        public Task<string> ComputeFileHashAsync(string filePath);
    }
}
