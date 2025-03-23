using FileMonitoringApp.FileTransferClient.Connection;
using FileMonitoringApp.Models.FileTransfer;
using Microsoft.Extensions.Options;

namespace FileMonitoringApp.FileTransferClient
{
    internal class MOVEitClient : IFileTransferClient
    {
        private readonly IFileTransferServiceConnection _fileTransferServiceConnection;

        public MOVEitClient(IFileTransferServiceConnection fileTransferServiceConnection,
            IOptions<FileTransferSettings> settingsOption)
        {
            fileTransferServiceConnection.SetBaseAddress(settingsOption.Value.BaseUrl);

            _fileTransferServiceConnection = fileTransferServiceConnection;
        }

        public async Task<bool> UploadAsync(string location)
        {
            var client = await _fileTransferServiceConnection.GetClientAsync();

            var response = await client.PostAsync("", null);

            return response.IsSuccessStatusCode;
        }
    }
}
