namespace FileMonitoringApp.FileTransferClient
{
    internal interface IFileTransferClient
    {
        Task<bool> UploadAsync(string location);
    }
}
