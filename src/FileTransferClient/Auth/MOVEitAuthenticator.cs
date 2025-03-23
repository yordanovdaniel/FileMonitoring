using FileMonitoringApp.Models;
using FileMonitoringApp.Models.FileTransfer;
using FileMonitoringApp.Services.Time;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace FileMonitoringApp.FileTransferClient.Auth
{
    internal class MOVEitAuthenticator : IFileTransferAuthenticator
    {
        private readonly ITimeService _timeService;
        private readonly FileTransferAuthSettings _settings;
        private TimeSpan? _expirationTimeBuffer;
        private DateTime? _expirationTime = null;
        private string? _refreshToken = null;

        public MOVEitAuthenticator(ITimeService timeService,
            IOptions<FileTransferAuthSettings> settingsOption)
        {
            _timeService = timeService;
            _settings = settingsOption.Value;

            _expirationTimeBuffer = TimeSpan.FromSeconds(5);
        }

        public Task RequireAuthentication(HttpClient httpClient)
        {
            if (!IsAuthenticated())
            {
                return AuthenticateAsync(httpClient);
            }

            if (!IsTokenAlive())
            {
                return RefreshTokenAsync(httpClient);
            }

            return Task.CompletedTask;
        }

        private bool IsAuthenticated()
        {
            return _expirationTime != null;
        }

        private bool IsTokenAlive()
        {
            return _expirationTime > _timeService.ProvideCurrentUtcTime() + _expirationTimeBuffer;
        }

        private Task AuthenticateAsync(HttpClient httpClient)
        {
            switch (_settings.GrantType)
            {
                case FileTransferGrantTypes.Password:
                    return AuthenticateAsync(httpClient, _settings.PasswordCredentials);
            }

            return Task.CompletedTask;
        }

        private Task AuthenticateAsync(HttpClient httpClient, FileTransferPasswordAuthCredentials? credentials)
        {
            ArgumentNullException.ThrowIfNull(credentials, nameof(credentials));
            ArgumentNullException.ThrowIfNull(credentials.Username, nameof(credentials.Username));
            ArgumentNullException.ThrowIfNull(credentials.Password, nameof(credentials.Password));

            var values = new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "username", credentials.Username },
                { "password", credentials.Password },
            };

            var content = new FormUrlEncodedContent(values);

            return AuthenticateAsync(httpClient, content);
        }

        private Task RefreshTokenAsync(HttpClient httpClient)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(_refreshToken, nameof(_refreshToken));

                var values = new Dictionary<string, string>
                {
                    { "grant_type", "refresh_token" },
                    { "refresh_token", _refreshToken },
                };

                var content = new FormUrlEncodedContent(values);

                return AuthenticateAsync(httpClient, content);
            }
            catch
            {
                ClearTokenInfo();
                throw;
            }
        }

        private void ClearTokenInfo()
        {
            _expirationTime = null;
            _refreshToken = null;
        }

        private async Task AuthenticateAsync(HttpClient httpClient, HttpContent content)
        {
            var response = await httpClient.PostAsync("token", content);

            response.EnsureSuccessStatusCode();

            var responseText = await response.Content.ReadAsStringAsync();
            var tokenData = JsonConvert.DeserializeObject<MOVEitTokenResponse>(responseText);

            UpdateTokenInfo(httpClient, tokenData);
        }

        private void UpdateTokenInfo(HttpClient httpClient, MOVEitTokenResponse? tokenData)
        {
            ArgumentNullException.ThrowIfNull(tokenData, nameof(tokenData));

            _refreshToken = tokenData.RefreshToken;
            _expirationTime = _timeService.ProvideCurrentUtcTime().AddSeconds(tokenData.ExpiresInSeconds);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenData.AccessToken);
        }
    }
}
