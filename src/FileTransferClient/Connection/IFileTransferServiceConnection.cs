namespace FileMonitoringApp.FileTransferClient.Connection
{
    internal interface IFileTransferServiceConnection
    {
        void SetBaseAddress(string address);

        Task<HttpClient> GetClientAsync(bool requireAuthentication = true);
    }
}
