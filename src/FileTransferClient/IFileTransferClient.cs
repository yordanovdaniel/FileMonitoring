using FileMonitoringApp.Models;

namespace FileMonitoringApp.FileTransferClient
{
    internal interface IFileTransferClient
    {
        Task<MOVEitUserDetailsResponse> GetCurrentUserDetailsAsync();

        Task<string> UploadAsync(string filePath, FileHashInfo fileHash, int folderId);

        Task<bool> DeleteAsync(string fileId);
    }
}
