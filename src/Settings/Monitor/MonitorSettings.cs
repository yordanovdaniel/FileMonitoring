namespace FileMonitoringApp.Settings.Monitor
{
    internal class MonitorSettings
    {
        public string FolderPath { get; set; } = string.Empty;

        public int DelayBetweenScansInSeconds { get; set; } = 1;
    }
}
