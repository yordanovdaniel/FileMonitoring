using Newtonsoft.Json;

namespace FileMonitoringApp.Models
{
    internal class FolderFiles
    {
        [JsonProperty("items")]
        public IEnumerable<FolderFileDetails> Files { get; set; } = [];

        [JsonRequired]
        [JsonProperty("paging")]
        public required PageDetails PagesInfo { get; set; }
    }
}
