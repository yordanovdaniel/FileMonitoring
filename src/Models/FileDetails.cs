using Newtonsoft.Json;

namespace FileMonitoringApp.Models
{
    internal class FileDetails
    {
        [JsonRequired]
        [JsonProperty("hash")]
        public required string Hash { get; set; }
    }
}
