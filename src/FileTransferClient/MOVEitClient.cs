using FileMonitoringApp.FileTransferClient.Connection;
using FileMonitoringApp.Models;
using FileMonitoringApp.Settings.FileTransfer;
using FileMonitoringApp.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace FileMonitoringApp.FileTransferClient
{
    internal class MOVEitClient : IFileTransferClient
    {
        private readonly IFileTransferServiceConnection _fileTransferServiceConnection;
        private readonly ILogger<MOVEitClient> _logger;

        public MOVEitClient(IFileTransferServiceConnection fileTransferServiceConnection,
            ILogger<MOVEitClient> logger,
            IOptions<FileTransferSettings> settingsOption)
        {
            fileTransferServiceConnection.SetBaseAddress(settingsOption.Value.BaseUrl);

            _fileTransferServiceConnection = fileTransferServiceConnection;
            _logger = logger;
        }

        public async Task<UserDetails> GetCurrentUserDetailsAsync()
        {
            var client = await _fileTransferServiceConnection.GetClientAsync();
            
            var response = await client.GetAsync("users/self");

            return await response.MapToAsync<UserDetails>();
        }

        public async Task<FileDetails> GetFileDetailsAsync(string fileId)
        {
            var client = await _fileTransferServiceConnection.GetClientAsync();

            var response = await client.GetAsync($"files/{fileId}");

            return await response.MapToAsync<FileDetails>();
        }

        public async Task<FolderFiles> GetFolderFilesAsync(int folderId, int page)
        {
            var client = await _fileTransferServiceConnection.GetClientAsync();

            var response = await client.GetAsync($"folders/{folderId}/files?page={page}");

            return await response.MapToAsync<FolderFiles>();
        }

        public async Task<string> UploadAsync(string filePath, FileHashInfo fileHash, int folderId)
        {
            var client = await _fileTransferServiceConnection.GetClientAsync();

            using var form = new MultipartFormDataContent();

            form.AddFile(filePath, "file");
            form.Add(new StringContent("hashtype"), fileHash.HashType);
            form.Add(new StringContent("hash"), fileHash.HashValue);

            var response = await client.PostAsync($"folders/{folderId}/files", form);

            var fileUploadResponse = await response.MapToAsync<FileUploadResponse>();

            _logger.LogInformation($"Uploaded file with id: {fileUploadResponse.FileId}");

            return fileUploadResponse.FileId;
        }

        public async Task<bool> DeleteAsync(string fileId)
        {
            var client = await _fileTransferServiceConnection.GetClientAsync();

            var response = await client.DeleteAsync($"files/{fileId}");

            _logger.LogInformation($"Deleted file with id: {fileId}");

            return response.IsSuccessStatusCode;
        }
    }
}
