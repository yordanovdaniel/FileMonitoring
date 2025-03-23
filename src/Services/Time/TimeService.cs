namespace FileMonitoringApp.Services.Time
{
    internal class TimeService : ITimeService
    {
        public DateTime ProvideCurrentUtcTime()
        {
            return DateTime.UtcNow;
        }
    }
}
