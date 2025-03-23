using FileMonitoringApp.FileTransferClient;
using FileMonitoringApp.Models;
using FileMonitoringApp.Services.FileHash;
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
        private readonly IDictionary<string, (string hash, string id)> _uploadedFiles;

        private readonly object _lock = new object();

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

            _uploadedFiles = new Dictionary<string, (string, string)>();
        }

        public async Task MonitorAsync()
        {
            var homeFolderId = await GetCurrentUserHomeFolderIdAsync();

            //TODO: load in _uploadedFiles already uploaded files

            while (true)
            {
                var filePaths = _fileScanningService.Scan(_monitorSettings.FolderPath);

                await UploadFilesAsync(filePaths, homeFolderId);
            }
        }

        private async Task<int> GetCurrentUserHomeFolderIdAsync()
        {
            var userDetails = await _fileTransferClient.GetCurrentUserDetailsAsync();

            return userDetails.HomeFolderId;
        }

        private Task UploadFilesAsync(IEnumerable<string> filePaths, int folderId)
        {
            return Parallel.ForEachAsync(filePaths, async (filePath, _) =>
            {
                var fileHash = await _fileHashService.ComputeFileHashAsync(filePath);
                var relativeFilePath = Path.GetRelativePath(_monitorSettings.FolderPath, filePath);

                if (!CheckIfFileIsUploaded(relativeFilePath, fileHash))
                {
                    await HandleUploadAsync(filePath, relativeFilePath, fileHash, folderId);
                }
            });
        }

        private bool CheckIfFileIsUploaded(string filePath, string fileHash)
        {
            lock (_lock)
            {
                if (!_uploadedFiles.ContainsKey(filePath) || _uploadedFiles[filePath].hash != fileHash)
                {
                    _uploadedFiles[filePath] = (fileHash, string.Empty);
                    return false;
                }
            }

            return true;
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

            lock (_lock)
            {
                if (string.IsNullOrEmpty(fileId))
                {
                    _uploadedFiles.Remove(relativeFilePath);
                    return;
                }

                _uploadedFiles[relativeFilePath] = (fileHash, fileId);
            }
        }
    }
}
