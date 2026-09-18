using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace SystemAdmin.CommonSetup.Security
{
    /// <summary>Cloudflare Turnstile 人机验证</summary>
    public class TurnstileService
    {
        private readonly HttpClient _httpClient;
        private readonly TurnstileOptions _options;

        public TurnstileService(HttpClient httpClient, IOptions<TurnstileOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        /// <summary>校验前端提交的 Turnstile Token</summary>
        public async Task<bool> VerifyAsync(string? token, string? remoteIp)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            var fields = new Dictionary<string, string>
            {
                ["secret"] = _options.SecretKey,
                ["response"] = token,
            };
            if (!string.IsNullOrWhiteSpace(remoteIp))
                fields["remoteip"] = remoteIp;

            using var response = await _httpClient.PostAsync(_options.VerifyUrl, new FormUrlEncodedContent(fields));
            if (!response.IsSuccessStatusCode)
                return false;

            var result = await response.Content.ReadFromJsonAsync<TurnstileVerifyResult>();
            return result?.Success ?? false;
        }

        private sealed class TurnstileVerifyResult
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }
        }
    }
}
