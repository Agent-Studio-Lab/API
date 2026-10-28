using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using API.Application.Abstractions;
using API.Domain.Enums;
using API.Domain.ValueObjects;

namespace API.Infrastructure.LLM;

public sealed class OpenAILikeLLMProvider : ILLMProvider
{
    private readonly HttpClient _httpClient;
    public LLMProviderType ProviderType { get; }

    public OpenAILikeLLMProvider(HttpClient httpClient, LLMProviderType providerType)
    {
        _httpClient = httpClient;
        ProviderType = providerType;
    }

    public async Task<string> GenerateResponseAsync(
        ModelConfig config,
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        var endpoint = ResolveEndpoint(config);
        var apiKey = config.ApiKey;

        var requestBody = new
        {
            model = config.ModelName,
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            temperature = config.Temperature
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        var json = JsonSerializer.Serialize(requestBody);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Lỗi gọi {ProviderType} API ({response.StatusCode}): {errorContent}");
        }

        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(responseString);
        var choices = doc.RootElement.GetProperty("choices");

        if (choices.GetArrayLength() == 0)
        {
            return string.Empty;
        }

        var content = choices[0].GetProperty("message").GetProperty("content").GetString();
        return content ?? string.Empty;
    }

    private string ResolveEndpoint(ModelConfig config)
    {
        if (!string.IsNullOrWhiteSpace(config.BaseUrl))
        {
            var url = config.BaseUrl.TrimEnd('/');
            return url.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase)
                ? url
                : $"{url}/chat/completions";
        }

        return ProviderType switch
        {
            LLMProviderType.DeepSeek => "https://api.deepseek.com/chat/completions",
            LLMProviderType.Ollama => "http://localhost:11434/v1/chat/completions",
            _ => "https://api.openai.com/v1/chat/completions"
        };
    }
}
