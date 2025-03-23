using FileMonitoringApp.Services.Scan;
using FileMonitoringApp.Services.Upload;
using Microsoft.Extensions.Logging;

namespace FileMonitoringApp.Services.Monitoring
{
    internal class FileMonitoringService : IFileMonitoringService
    {
        private readonly IFileScanningService _fileScanningService;
        private readonly IFileUploadingService _fileUploadingService;
        private readonly ILogger<FileMonitoringService> _logger;
        private readonly ISet<string> _uploadedFiles;

        private readonly object _lock = new object();

        public FileMonitoringService(IFileScanningService fileScanningService,
            IFileUploadingService fileUploadingService,
            ILogger<FileMonitoringService> logger)
        {
            _fileScanningService = fileScanningService;
            _fileUploadingService = fileUploadingService;
            _logger = logger;

            _uploadedFiles = new HashSet<string>();
        }

        public void Monitor()
        {
            while (true)
            {
                var files = _fileScanningService.Scan();

                Task.Run(() => Parallel.ForEachAsync(files, async (file, _) =>
                {
                    bool fileUploaded;
                    lock (_lock)
                    {
                        fileUploaded = !_uploadedFiles.Add(file);
                    }

                    if (fileUploaded)
                    {
                        await HandleUploadAsync(file);
                    }
                }));
            }
        }

        private async Task HandleUploadAsync(string fileLocation)
        {
            bool isUploaded;
            try
            {
                isUploaded = await _fileUploadingService.UploadAsync(fileLocation);
            }
            catch (Exception ex)
            {
                isUploaded = false;
                _logger.LogError($"File: {fileLocation} couldn't upload, exception: {ex}");
            }

            if (!isUploaded)
            {
                lock (_lock)
                {
                    _uploadedFiles.Remove(fileLocation);
                }
            }
        }
    }
}
