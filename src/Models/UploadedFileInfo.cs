namespace FileMonitoringApp.Models
{
    internal class UploadedFileInfo(string id, string hash)
    {
        public string Id { get; } = id;

        public string Hash { get; } = hash;
    }
}
