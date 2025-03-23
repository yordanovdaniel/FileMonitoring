namespace FileMonitoringApp.Services.Scan
{
    internal class FileSystemScanningService : IFileScanningService
    {
        public IEnumerable<string> Scan(string folderPath)
        {
            return Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
        }
    }
}
