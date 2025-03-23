using Newtonsoft.Json;

namespace FileMonitoringApp.Models
{
    internal class FolderFileDetails
    {
        [JsonRequired]
        [JsonProperty("id")]
        public required string Id { get; set; }

        [JsonRequired]
        [JsonProperty("path")]
        public required string Path { get; set; }
    }
}
