namespace FileMonitoringApp.Services.Scan
{
    internal interface IFileScanningService
    {
        IEnumerable<string> Scan(string folderPath);
    }
}
