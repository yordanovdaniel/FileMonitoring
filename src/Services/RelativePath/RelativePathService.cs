namespace FileMonitoringApp.Services.RelativePath
{
    internal class RelativePathService : IRelativePathService
    {
        public string GetRelativePathOfCloudPath(string path)
        {
            int slashesCount = 0;
            for (int i = 0; i < path.Length; i++)
            {
                if (path[i] == '/')
                {
                    slashesCount++;

                    if (slashesCount == 3)
                    {
                        return path[(i + 1)..];
                    }
                }
            }

            throw new ArgumentException($"Cloud path {path} is invalid!");
        }

        public string GetRelativePathOfLocalPath(string basePath, string path)
        {
            return Path.GetRelativePath(basePath, path);
        }
    }
}
