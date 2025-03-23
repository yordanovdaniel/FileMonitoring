namespace FileMonitoringApp.FileTransferClient.Auth
{
    internal interface IFileTransferAuthenticator
    {
        Task RequireAuthentication(HttpClient httpClient);
    }
}
