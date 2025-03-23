using FileMonitoringApp.FileTransferClient.Connection;
using FileMonitoringApp.Models;
using FileMonitoringApp.Settings.FileTransfer;
using FileMonitoringApp.Extensions;
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

        public async Task<MOVEitUserDetailsResponse> GetCurrentUserDetailsAsync()
        {
            var client = await _fileTransferServiceConnection.GetClientAsync();
            
            var response = await client.GetAsync("users/self");

            return await response.MapToAsync<MOVEitUserDetailsResponse>();
        }

        public async Task<string> UploadAsync(string filePath, FileHashInfo fileHash, int folderId)
        {
            var client = await _fileTransferServiceConnection.GetClientAsync();

            using var form = new MultipartFormDataContent();

            form.AddFile(filePath, "file");
            form.Add(new StringContent("hashtype"), fileHash.HashType);
            form.Add(new StringContent("hash"), fileHash.HashValue);

            var response = await client.PostAsync($"folders/{folderId}/files", form);

            var fileUploadResponse = await response.MapToAsync<MOVEitFileUploadResponse>();

            return fileUploadResponse.FileId;
        }
    }
}
