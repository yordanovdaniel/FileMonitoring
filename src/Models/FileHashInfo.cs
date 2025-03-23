namespace FileMonitoringApp.Models
{
    internal class FileHashInfo(string hash, string hashType)
    {
        public string HashValue { get; } = hash;

        public string HashType { get; } = hashType;
    }
}
