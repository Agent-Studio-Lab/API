using System.Text;
using System.Text.Json;
using API.Application.Abstractions;
using API.Domain.Common;
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
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return await new MockLLMProvider(ProviderType).GenerateResponseAsync(config, systemPrompt, userPrompt, cancellationToken);
        }

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
            throw new HttpRequestException($"Gemini API request failed ({response.StatusCode}): {errorContent}");
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

    public async Task<Result<bool>> ValidateConnectionAsync(
        ModelConfig config,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(config.ApiKey))
        {
            return Result<bool>.Failure(400, "API Key của Google Gemini không được để trống khi xác thực kết nối.");
        }

        var model = string.IsNullOrWhiteSpace(config.ModelName) ? "gemini-1.5-flash" : config.ModelName;
        var baseUrl = !string.IsNullOrWhiteSpace(config.BaseUrl)
            ? config.BaseUrl.TrimEnd('/')
            : "https://generativelanguage.googleapis.com";

        var url = $"{baseUrl}/v1beta/models/{model}:generateContent?key={config.ApiKey}";

        var requestBody = new Dictionary<string, object>
        {
            ["contents"] = new[]
            {
                new
                {
                    parts = new[] { new { text = "ping" } }
                }
            },
            ["generationConfig"] = new
            {
                maxOutputTokens = 1
            }
        };

        try
        {
            var json = JsonSerializer.Serialize(requestBody);
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return Result<bool>.Success(true);
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var detailedMessage = TryExtractErrorMessage(errorContent) ?? errorContent;

            return (int)response.StatusCode switch
            {
                400 => Result<bool>.Failure(400, $"API Key hoặc cấu hình Gemini không hợp lệ: {detailedMessage}"),
                403 => Result<bool>.Failure(403, $"API Key không có quyền sử dụng Gemini hoặc chưa kích hoạt API: {detailedMessage}"),
                404 => Result<bool>.Failure(404, $"Không tìm thấy Model Gemini '{model}': {detailedMessage}"),
                429 => Result<bool>.Failure(429, $"Vượt quá giới hạn tần suất hoặc hạn ngạch Google Gemini: {detailedMessage}"),
                _ => Result<bool>.Failure((int)response.StatusCode, $"Xác thực Google Gemini thất bại ({response.StatusCode}): {detailedMessage}")
            };
        }
        catch (HttpRequestException ex)
        {
            return Result<bool>.Failure(502, $"Không thể kết nối đến Google Gemini API: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return Result<bool>.Failure(408, "Quá thời gian kết nối (Timeout) đến Google Gemini API. Vui lòng kiểm tra lại kết nối mạng.");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(500, $"Lỗi không xác định khi kiểm tra Google Gemini API: {ex.Message}");
        }
    }

    private static string? TryExtractErrorMessage(string jsonContent)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonContent);
            if (doc.RootElement.TryGetProperty("error", out var errorElement))
            {
                if (errorElement.ValueKind == JsonValueKind.Object && errorElement.TryGetProperty("message", out var msg))
                {
                    return msg.GetString();
                }
                if (errorElement.ValueKind == JsonValueKind.String)
                {
                    return errorElement.GetString();
                }
            }
        }
        catch
        {
            // Fallback to raw response if not standard JSON error
        }
        return null;
    }
}
