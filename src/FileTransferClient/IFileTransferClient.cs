using FileMonitoringApp.Models;

namespace FileMonitoringApp.FileTransferClient
{
    internal interface IFileTransferClient
    {
        Task<UserDetails> GetCurrentUserDetailsAsync();

        Task<FileDetails> GetFileDetailsAsync(string fileId);

        Task<FolderFiles> GetFolderFilesAsync(int folderId, int page);

        Task<string> UploadAsync(string filePath, FileHashInfo fileHash, int folderId);

        Task<bool> DeleteAsync(string fileId);
    }
}
