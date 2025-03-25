using FileMonitoringApp.FileTransferClient;
using FileMonitoringApp.Models;
using FileMonitoringApp.Services.FileHash;
using FileMonitoringApp.Services.Monitor;
using FileMonitoringApp.Services.RelativePath;
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
        private readonly IRelativePathService _relativePathService;
        private readonly ILogger<FileMonitoringService> _logger;
        private readonly MonitorSettings _monitorSettings;

        private readonly UploadedFilesRegistry _uploadedFilesRegistry;

        public FileMonitoringService(IFileScanningService fileScanningService,
            IFileTransferClient fileTransferClient,
            IFileHashService fileHashService,
            IRelativePathService relativePathService,
            ILogger<FileMonitoringService> logger,
            IOptions<MonitorSettings> settingsOption)
        {
            _fileScanningService = fileScanningService;
            _fileTransferClient = fileTransferClient;
            _fileHashService = fileHashService;
            _relativePathService = relativePathService;
            _logger = logger;
            _monitorSettings = settingsOption.Value;

            _uploadedFilesRegistry = new UploadedFilesRegistry();
        }

        public async Task MonitorAsync()
        {
            var delayBetweenScans = GetDelayBetweenScans();
            var homeFolderId = await GetCurrentUserHomeFolderIdAsync();

            await SeedUploadedFilesAsync(homeFolderId);

            while (true)
            {
                var filePaths = _fileScanningService.Scan(_monitorSettings.FolderPath);

                await UploadFilesAsync(filePaths, homeFolderId);
                await DeleteFilesAsync(filePaths);

                await Task.Delay(delayBetweenScans);
            }
        }

        private TimeSpan GetDelayBetweenScans()
        {
            if (_monitorSettings.DelayBetweenScansInSeconds < 0)
            {
                throw new ArgumentOutOfRangeException($"{nameof(_monitorSettings.DelayBetweenScansInSeconds)} can\'t be negative!");
            }

            return TimeSpan.FromSeconds(_monitorSettings.DelayBetweenScansInSeconds);
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
                    var relativeFilePath = _relativePathService.GetRelativePathOfCloudPath(file.Path);

                    _uploadedFilesRegistry.SetFileAsUploaded(relativeFilePath, new UploadedFileInfo(file.Id, fileDetails.Hash));
                });
            } while (++page < pagesCount);
        }

        private Task DeleteFilesAsync(IEnumerable<string> filePaths)
        {
            var relativeFilePaths = filePaths
                .Select(filePath => _relativePathService.GetRelativePathOfLocalPath(_monitorSettings.FolderPath, filePath));

            return Parallel.ForEachAsync(_uploadedFilesRegistry.GetDeletedFiles(relativeFilePaths), async (filePath, _) =>
            {
                var fileInfo = _uploadedFilesRegistry.GetFile(filePath);

                await HandleDeleteAsync(filePath, fileInfo);
            });
        }

        private Task UploadFilesAsync(IEnumerable<string> filePaths, int folderId)
        {
            return Parallel.ForEachAsync(filePaths, async (filePath, _) =>
            {
                try
                {
                    var fileHash = await _fileHashService.ComputeFileHashAsync(filePath);
                    var relativeFilePath = _relativePathService.GetRelativePathOfLocalPath(_monitorSettings.FolderPath, filePath);

                    if (!_uploadedFilesRegistry.CheckIfFileIsUploaded(relativeFilePath, fileHash, out var fileForDelete))
                    {
                        if (fileForDelete != null && !await HandleDeleteAsync(relativeFilePath, fileForDelete))
                        {
                            return;
                        }

                        await HandleUploadAsync(filePath, relativeFilePath, fileHash, folderId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            });
        }

        private async Task<bool> HandleDeleteAsync(string relativeFilePath, UploadedFileInfo uploadedFile)
        {
            var isDeleted = await _fileTransferClient.DeleteAsync(uploadedFile.Id);

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
            var fileHashInfo = new FileHashInfo(fileHash, _fileHashService.HashType);
            var fileId = await _fileTransferClient.UploadAsync(filePath, fileHashInfo, folderId);

            if (string.IsNullOrEmpty(fileId))
            {
                _uploadedFilesRegistry.RemoveFile(relativeFilePath);
                return;
            }

            _uploadedFilesRegistry.SetFileAsUploaded(relativeFilePath, new UploadedFileInfo(fileId, fileHash));
        }
    }
}
