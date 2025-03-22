using FileMonitoringApp.Services.Scan;
using FileMonitoringApp.Services.Upload;

namespace FileMonitoringApp.Services.Monitoring
{
    internal class FileMonitoringService : IFileMonitoringService
    {
        private readonly IFileScanningService _fileScanningService;
        private readonly IFileUploadingService _fileUploadingService;
        private readonly ISet<string> _uploadedFiles;
        private readonly object _lock = new object();

        public FileMonitoringService(IFileScanningService fileScanningService,
            IFileUploadingService fileUploadingService
            /*TODO: ILog*/)
        {
            _fileScanningService = fileScanningService;
            _fileUploadingService = fileUploadingService;
            _uploadedFiles = new HashSet<string>();
        }

        public void Monitor()
        {
            while (true)
            {
                var files = _fileScanningService.Scan();

                Parallel.ForEachAsync(files, async (file, _) =>
                {
                    bool fileIsUploaded;
                    lock (_lock)
                    {
                        fileIsUploaded = !_uploadedFiles.Add(file);
                    }

                    if (fileIsUploaded)
                    {
                        await HandleUploadAsync(file);
                    }
                });
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
                //TODO: log ex
            }

            if(!isUploaded)
            {
                lock (_lock)
                {
                    _uploadedFiles.Remove(fileLocation);
                }
            }
        }
    }
}
