using Newtonsoft.Json;

namespace FileMonitoringApp.Models
{
    internal class UserDetails
    {
        [JsonRequired]
        [JsonProperty("homeFolderID")]
        public int HomeFolderId { get; set; }
    }
}
