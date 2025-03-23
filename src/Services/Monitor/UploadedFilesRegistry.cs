using FileMonitoringApp.Models;

namespace FileMonitoringApp.Services.Monitor
{
    internal class UploadedFilesRegistry
    {
        private readonly IDictionary<string, UploadedFileInfo> _uploadedFiles;

        public UploadedFilesRegistry()
        {
            _uploadedFiles = new Dictionary<string, UploadedFileInfo>();
        }

        public void SetFileAsUploaded(string filePath, UploadedFileInfo file)
        {
            _uploadedFiles[filePath] = file;
        }

        public UploadedFileInfo GetFile(string filePath)
        {
            return _uploadedFiles[filePath];
        }

        public void RemoveFile(string filePath)
        {
            _uploadedFiles.Remove(filePath);
        }

        public bool CheckIfFileIsUploaded(string filePath, string fileHash, out UploadedFileInfo? fileForDelete)
        {
            fileForDelete = null;

            if (!_uploadedFiles.ContainsKey(filePath))
            {
                fileForDelete = _uploadedFiles[filePath];
                return false;
            }

            if (_uploadedFiles[filePath].Hash != fileHash)
            {
                return false;
            }

            return true;
        }

        internal IEnumerable<string> GetDeletedFiles(IEnumerable<string> filePaths)
        {
            var filesSet = filePaths.ToHashSet();

            return _uploadedFiles.Keys.Where(filePath => !filesSet.Contains(filePath));
        }
    }
}
