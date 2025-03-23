using Newtonsoft.Json;

namespace FileMonitoringApp.Extensions
{
    internal static class HttpResponseExtensions
    {
        public static async Task<T> MapToAsync<T>(this HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();

            var responseText = await response.Content.ReadAsStringAsync();

            var deserializedResponse = JsonConvert.DeserializeObject<T>(responseText);

            ArgumentNullException.ThrowIfNull(deserializedResponse, nameof(deserializedResponse));

            return deserializedResponse;
        }
    }
}
