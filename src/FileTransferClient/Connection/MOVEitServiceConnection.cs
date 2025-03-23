using FileMonitoringApp.FileTransferClient.Auth;

namespace FileMonitoringApp.FileTransferClient.Connection
{
    internal class MOVEitServiceConnection : IFileTransferServiceConnection
    {
        private readonly IFileTransferAuthenticator _fileTransferAuthenticator;
        private readonly HttpClient _httpClient;

        public MOVEitServiceConnection(IFileTransferAuthenticator fileTransferAuthenticator)
        {
            _fileTransferAuthenticator = fileTransferAuthenticator;
            
            _httpClient = new HttpClient();
        }

        public void SetBaseAddress(string address)
        {
            _httpClient.BaseAddress = new Uri(address);
        }

        public async Task<HttpClient> GetClientAsync(bool requireAuthentication)
        {
            if (requireAuthentication)
            {
                await _fileTransferAuthenticator.RequireAuthentication(_httpClient);
            }

            return _httpClient;
        }
    }
}
