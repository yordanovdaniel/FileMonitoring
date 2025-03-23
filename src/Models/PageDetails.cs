using Newtonsoft.Json;

namespace FileMonitoringApp.Models
{
    internal class PageDetails
    {
        [JsonRequired]
        [JsonProperty("totalPages")]
        public int TotalPages { get; set; }
    }
}
