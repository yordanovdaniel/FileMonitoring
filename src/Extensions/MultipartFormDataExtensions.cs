using System.Net.Http.Headers;

namespace FileMonitoringApp.Extensions
{
    internal static class MultipartFormDataExtensions
    {
        public static void AddFile(this MultipartFormDataContent form, string filePath, string fileFormName)
        {
            var fileContent = new StreamContent(new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                Headers =
                {
                    ContentType = new MediaTypeHeaderValue("application/octet-stream")
                }
            };

            form.Add(fileContent, fileFormName, Path.GetFileName(filePath));
        }
    }
}
