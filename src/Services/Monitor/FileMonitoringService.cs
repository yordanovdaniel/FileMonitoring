using FileMonitoringApp.FileTransferClient;
using FileMonitoringApp.Models;
using FileMonitoringApp.Services.FileHash;
using FileMonitoringApp.Services.Monitor;
using FileMonitoringApp.Services.Scan;
using FileMonitoringApp.Settings.Monitor;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileMonitoringApp.Services.Monitoring
{
    internal class FileMonitoringService : IFileMonitoringService
    {
        private readonly IFileScanningService _fileScanningService;
        private readonly IFileTransferClient _fileTransferClient;
        private readonly IFileHashService _fileHashService;
        private readonly ILogger<FileMonitoringService> _logger;
        private readonly MonitorSettings _monitorSettings;
        // key -> file path, value -> (file hash, file id)

        private readonly UploadedFilesRegistry _uploadedFilesRegistry;

        public FileMonitoringService(IFileScanningService fileScanningService,
            IFileTransferClient fileTransferClient,
            IFileHashService fileHashService,
            ILogger<FileMonitoringService> logger,
            IOptions<MonitorSettings> settingsOption)
        {
            _fileScanningService = fileScanningService;
            _fileTransferClient = fileTransferClient;
            _fileHashService = fileHashService;
            _logger = logger;
            _monitorSettings = settingsOption.Value;

            _uploadedFilesRegistry = new UploadedFilesRegistry();
        }

        public async Task MonitorAsync()
        {
            var homeFolderId = await GetCurrentUserHomeFolderIdAsync();

            await SeedUploadedFilesAsync(homeFolderId);

            while (true)
            {
                var filePaths = _fileScanningService.Scan(_monitorSettings.FolderPath);

                await UploadFilesAsync(filePaths, homeFolderId);
                await DeleteFilesAsync(filePaths);
            }
        }

        private async Task<int> GetCurrentUserHomeFolderIdAsync()
        {
            var userDetails = await _fileTransferClient.GetCurrentUserDetailsAsync();

            return userDetails.HomeFolderId;
        }

        private async Task SeedUploadedFilesAsync(int folderId)
        {
            var page = 1;
            int pagesCount;
            do
            {
                var folderInfo = await _fileTransferClient.GetFolderFilesAsync(folderId, page);
                pagesCount = folderInfo.PagesInfo.TotalPages;

                await Parallel.ForEachAsync(folderInfo.Files, async (file, _) =>
                {
                    var fileDetails = await _fileTransferClient.GetFileDetailsAsync(file.Id);
                    var relativeFilePath = GetRelativePathOfCloudPath(file.Path);

                    _uploadedFilesRegistry.SetFileAsUploaded(relativeFilePath, new UploadedFileInfo(file.Id, fileDetails.Hash));
                });
            } while (++page < pagesCount);
        }

        private string GetRelativePathOfCloudPath(string path)
        {
            int slashesCount = 0;
            for (int i = 0; i < path.Length; i++)
            {
                if (path[i] == '/')
                {
                    slashesCount++;

                    if(slashesCount == 3)
                    {
                        return path[(i + 1)..];
                    }
                }
            }

            throw new ArgumentException($"Cloud path {path} is invalid!");
        }

        private Task DeleteFilesAsync(IEnumerable<string> filePaths)
        {
            var relativeFilePaths = filePaths.Select(GetRelativePathOfLocalPath);

            return Parallel.ForEachAsync(_uploadedFilesRegistry.GetDeletedFiles(relativeFilePaths), async (filePath, _) =>
            {
                var relativeFilePath = GetRelativePathOfLocalPath(filePath);
                var fileInfo = _uploadedFilesRegistry.GetFile(filePath);

                await HandleDeleteAsync(relativeFilePath, fileInfo);
            });
        }

        private Task UploadFilesAsync(IEnumerable<string> filePaths, int folderId)
        {
            return Parallel.ForEachAsync(filePaths, async (filePath, _) =>
            {
                var fileHash = await _fileHashService.ComputeFileHashAsync(filePath);
                var relativeFilePath = GetRelativePathOfLocalPath(filePath);

                if (!_uploadedFilesRegistry.CheckIfFileIsUploaded(relativeFilePath, fileHash, out var fileForDelete))
                {
                    if (fileForDelete != null && !await HandleDeleteAsync(relativeFilePath, fileForDelete))
                    {
                        return;
                    }

                    await HandleUploadAsync(filePath, relativeFilePath, fileHash, folderId);
                }
            });
        }

        private string GetRelativePathOfLocalPath(string path)
        {
            return Path.GetRelativePath(_monitorSettings.FolderPath, path);
        }

        private async Task<bool> HandleDeleteAsync(string relativeFilePath, UploadedFileInfo uploadedFile)
        {
            var isDeleted = false;
            try
            {
                isDeleted = await _fileTransferClient.DeleteAsync(uploadedFile.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to delete file with id {uploadedFile.Id}. Exception details: {ex}");
            }

            if (isDeleted)
            {
                _uploadedFilesRegistry.RemoveFile(relativeFilePath);
            }
            else
            {
                _uploadedFilesRegistry.SetFileAsUploaded(relativeFilePath, uploadedFile);
            }

            return isDeleted;
        }

        private async Task HandleUploadAsync(string filePath, string relativeFilePath, string fileHash, int folderId)
        {
            string? fileId = null;
            try
            {
                var fileHashInfo = new FileHashInfo(fileHash, _fileHashService.HashType);
                fileId = await _fileTransferClient.UploadAsync(filePath, fileHashInfo, folderId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to upload file with path {filePath}. Exception details: {ex}");
            }

            if (string.IsNullOrEmpty(fileId))
            {
                _uploadedFilesRegistry.RemoveFile(relativeFilePath);
                return;
            }

            _uploadedFilesRegistry.SetFileAsUploaded(relativeFilePath, new UploadedFileInfo(fileHash, fileId));
        }
    }
}
