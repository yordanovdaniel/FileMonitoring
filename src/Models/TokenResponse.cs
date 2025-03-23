using Newtonsoft.Json;

namespace FileMonitoringApp.Models
{
    internal class TokenResponse
    {
        [JsonRequired]
        [JsonProperty("access_token")]
        public required string AccessToken { get; set; }

        [JsonRequired]
        [JsonProperty("expires_in")]
        public int ExpiresInSeconds { get; set; }

        [JsonRequired]
        [JsonProperty("refresh_token")]
        public required string RefreshToken { get; set; }
    }
}
