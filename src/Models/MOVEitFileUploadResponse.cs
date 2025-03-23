using Newtonsoft.Json;

namespace FileMonitoringApp.Models
{
    internal class MOVEitFileUploadResponse
    {
        [JsonRequired]
        [JsonProperty("id")]
        public required string FileId { get; set; }
    }
}
