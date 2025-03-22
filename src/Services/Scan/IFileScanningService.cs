namespace FileMonitoringApp.Services.Scan
{
    internal interface IFileScanningService
    {
        public IEnumerable<string> Scan();
    }
}
