namespace FileMonitoringApp.Services.Upload
{
    internal interface IFileUploadingService
    {
        public Task<bool> UploadAsync(string location);
    }
}
