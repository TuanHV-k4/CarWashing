using System.Net.Http.Json;
using System.Text.Json;
using BusinessLayer.Helpers;
using BusinessLayer.IService.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BusinessLayer.Service.AI
{
    public class GeminiClient : IGenerativeAIClient
    {
        private readonly HttpClient _httpClient;
        private readonly GeminiSettings _settings;
        private readonly ILogger<GeminiClient> _logger;

        public GeminiClient(HttpClient httpClient, IOptions<GeminiSettings> settings, ILogger<GeminiClient> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
            _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
        }

        public bool ShouldUseMock =>
            _settings.UseMockFallback || string.IsNullOrWhiteSpace(_settings.ApiKey);

        public async Task<(string Text, string Source)> GenerateAsync(
            string systemPrompt,
            IEnumerable<(string Role, string Content)> messages,
            CancellationToken cancellationToken = default)
        {
            if (ShouldUseMock)
                return (string.Empty, "mock");

            try
            {
                var contents = messages.Select(m => new
                {
                    role = m.Role == "assistant" ? "model" : "user",
                    parts = new[] { new { text = m.Content } }
                }).ToList();

                var requestBody = new
                {
                    system_instruction = new { parts = new[] { new { text = systemPrompt } } },
                    contents
                };

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_settings.Model}:generateContent?key={_settings.ApiKey}";
                using var response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning("Gemini API error {Status}: {Error}", response.StatusCode, error);
                    return (string.Empty, "fallback");
                }

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return (text ?? string.Empty, "gemini");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(ex, "Gemini request timed out after {Seconds}s", _settings.TimeoutSeconds);
                return (string.Empty, "fallback");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Gemini request failed");
                return (string.Empty, "fallback");
            }
        }
    }
}
