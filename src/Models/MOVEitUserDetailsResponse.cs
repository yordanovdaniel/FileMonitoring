using Newtonsoft.Json;

namespace FileMonitoringApp.Models
{
    internal class MOVEitUserDetailsResponse
    {
        [JsonRequired]
        [JsonProperty("homeFolderID")]
        public int HomeFolderId { get; set; }
    }
}
