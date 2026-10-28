using System.Text;
using System.Text.Json;
using API.Application.Abstractions;
using API.Domain.Enums;
using API.Domain.ValueObjects;

namespace API.Infrastructure.LLM;

public sealed class GeminiLLMProvider : ILLMProvider
{
    private readonly HttpClient _httpClient;
    public LLMProviderType ProviderType => LLMProviderType.Gemini;

    public GeminiLLMProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GenerateResponseAsync(
        ModelConfig config,
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        var model = string.IsNullOrWhiteSpace(config.ModelName) ? "gemini-1.5-flash" : config.ModelName;
        var apiKey = config.ApiKey;

        var baseUrl = !string.IsNullOrWhiteSpace(config.BaseUrl)
            ? config.BaseUrl.TrimEnd('/')
            : "https://generativelanguage.googleapis.com";

        var url = $"{baseUrl}/v1beta/models/{model}:generateContent?key={apiKey}";

        var requestBody = new Dictionary<string, object>
        {
            ["system_instruction"] = new
            {
                parts = new[] { new { text = systemPrompt } }
            },
            ["contents"] = new[]
            {
                new
                {
                    parts = new[] { new { text = userPrompt } }
                }
            }
        };

        if (config.Temperature.HasValue)
        {
            requestBody["generationConfig"] = new
            {
                temperature = config.Temperature.Value
            };
        }

        var json = JsonSerializer.Serialize(requestBody);
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Lỗi gọi Gemini API ({response.StatusCode}): {errorContent}");
        }

        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(responseString);

        if (!doc.RootElement.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
        {
            return string.Empty;
        }

        var firstCandidate = candidates[0];
        if (!firstCandidate.TryGetProperty("content", out var contentObj) ||
            !contentObj.TryGetProperty("parts", out var parts) ||
            parts.GetArrayLength() == 0)
        {
            return string.Empty;
        }

        return parts[0].GetProperty("text").GetString() ?? string.Empty;
    }
}
