using System.Text;
using System.Text.Json;
using API.Application.Abstractions;
using API.Domain.Common;
using API.Domain.Enums;
using API.Domain.ValueObjects;

namespace API.Infrastructure.LLM;

public sealed class AnthropicLLMProvider : ILLMProvider
{
    private readonly HttpClient _httpClient;
    public LLMProviderType ProviderType => LLMProviderType.Anthropic;

    public AnthropicLLMProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GenerateResponseAsync(
        ModelConfig config,
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        var apiKey = config.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return await new MockLLMProvider(ProviderType).GenerateResponseAsync(config, systemPrompt, userPrompt, cancellationToken);
        }

        var model = string.IsNullOrWhiteSpace(config.ModelName) ? "claude-3-5-sonnet-20241022" : config.ModelName;
        var endpoint = !string.IsNullOrWhiteSpace(config.BaseUrl)
            ? $"{config.BaseUrl.TrimEnd('/')}/v1/messages"
            : "https://api.anthropic.com/v1/messages";

        var requestBody = new Dictionary<string, object>
        {
            ["model"] = model,
            ["max_tokens"] = 4096,
            ["messages"] = new object[]
            {
                new { role = "user", content = userPrompt }
            }
        };

        if (!string.IsNullOrWhiteSpace(systemPrompt))
        {
            requestBody["system"] = systemPrompt;
        }

        if (config.Temperature.HasValue)
        {
            requestBody["temperature"] = config.Temperature.Value;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Add("x-api-key", apiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");

        var json = JsonSerializer.Serialize(requestBody);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Anthropic API request failed ({response.StatusCode}): {errorContent}");
        }

        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(responseString);
        if (doc.RootElement.TryGetProperty("content", out var contentArray) && contentArray.GetArrayLength() > 0)
        {
            var text = contentArray[0].GetProperty("text").GetString();
            return text ?? string.Empty;
        }

        return string.Empty;
    }

    public async Task<Result<bool>> ValidateConnectionAsync(
        ModelConfig config,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(config.ApiKey))
        {
            return Result<bool>.Failure(400, "API Key của Anthropic không được để trống khi xác thực kết nối.");
        }

        var model = string.IsNullOrWhiteSpace(config.ModelName) ? "claude-3-5-sonnet-20241022" : config.ModelName;
        var endpoint = !string.IsNullOrWhiteSpace(config.BaseUrl)
            ? $"{config.BaseUrl.TrimEnd('/')}/v1/messages"
            : "https://api.anthropic.com/v1/messages";

        var requestBody = new Dictionary<string, object>
        {
            ["model"] = model,
            ["max_tokens"] = 1,
            ["messages"] = new object[]
            {
                new { role = "user", content = "ping" }
            }
        };

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Add("x-api-key", config.ApiKey);
            request.Headers.Add("anthropic-version", "2023-06-01");

            var json = JsonSerializer.Serialize(requestBody);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return Result<bool>.Success(true);
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var detailedMessage = TryExtractErrorMessage(errorContent) ?? errorContent;

            return (int)response.StatusCode switch
            {
                401 => Result<bool>.Failure(401, $"API Key Anthropic không hợp lệ: {detailedMessage}"),
                404 => Result<bool>.Failure(404, $"Không tìm thấy Model Anthropic '{model}': {detailedMessage}"),
                429 => Result<bool>.Failure(429, $"Tài khoản Anthropic hết hạn ngạch hoặc bị giới hạn tốc độ: {detailedMessage}"),
                _ => Result<bool>.Failure((int)response.StatusCode, $"Xác thực Anthropic thất bại ({response.StatusCode}): {detailedMessage}")
            };
        }
        catch (HttpRequestException ex)
        {
            return Result<bool>.Failure(502, $"Không thể kết nối đến Anthropic API: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return Result<bool>.Failure(408, "Quá thời gian kết nối (Timeout) đến Anthropic API.");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(500, $"Lỗi không xác định khi kiểm tra Anthropic API: {ex.Message}");
        }
    }

    private static string? TryExtractErrorMessage(string jsonContent)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonContent);
            if (doc.RootElement.TryGetProperty("error", out var errorElement) &&
                errorElement.TryGetProperty("message", out var msg))
            {
                return msg.GetString();
            }
        }
        catch { }
        return null;
    }
}
