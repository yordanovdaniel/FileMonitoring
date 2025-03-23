namespace FileMonitoringApp.Services.RelativePath
{
    internal interface IRelativePathService
    {
        string GetRelativePathOfCloudPath(string path);

        string GetRelativePathOfLocalPath(string basePath, string path);
    }
}
