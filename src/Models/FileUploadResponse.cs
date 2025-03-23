using Newtonsoft.Json;

namespace FileMonitoringApp.Models
{
    internal class FileUploadResponse
    {
        [JsonRequired]
        [JsonProperty("id")]
        public required string FileId { get; set; }
    }
}
