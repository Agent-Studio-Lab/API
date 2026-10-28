using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using API.Application.Abstractions;
using API.Domain.Common;
using API.Domain.ValueObjects;

namespace API.Infrastructure.LLM;

public sealed class OpenAILikeLLMProvider : ILLMProvider
{
    private readonly HttpClient _httpClient;
    public string ProviderName { get; }

    public OpenAILikeLLMProvider(HttpClient httpClient, string providerName = "OpenAI")
    {
        _httpClient = httpClient;
        ProviderName = string.IsNullOrWhiteSpace(providerName) ? "OpenAI" : providerName;
    }

    public async Task<string> GenerateResponseAsync(
        ModelConfig config,
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        var endpoint = ResolveEndpoint(config);
        var apiKey = config.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey) && !IsLocalProvider())
        {
            return await new MockLLMProvider(ProviderName).GenerateResponseAsync(config, systemPrompt, userPrompt, cancellationToken);
        }

        var requestBody = new Dictionary<string, object>
        {
            ["model"] = config.ModelName,
            ["messages"] = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            }
        };

        if (config.Temperature.HasValue)
        {
            requestBody["temperature"] = config.Temperature.Value;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        if (ProviderName.Equals("openrouter", StringComparison.OrdinalIgnoreCase))
        {
            request.Headers.TryAddWithoutValidation("HTTP-Referer", "https://agentstudio.lab");
            request.Headers.TryAddWithoutValidation("X-Title", "Agent Studio");
        }

        var json = JsonSerializer.Serialize(requestBody);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Provider {ProviderName} request failed ({response.StatusCode}): {errorContent}");
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

    public async Task<Result<bool>> ValidateConnectionAsync(
        ModelConfig config,
        CancellationToken cancellationToken = default)
    {
        if (!IsLocalProvider() && string.IsNullOrWhiteSpace(config.ApiKey))
        {
            return Result<bool>.Failure(400, $"API Key của {ProviderName} không được để trống khi xác thực kết nối.");
        }

        if (string.IsNullOrWhiteSpace(config.ModelName))
        {
            return Result<bool>.Failure(400, "Tên Model không được để trống khi xác thực kết nối.");
        }

        var endpoint = ResolveEndpoint(config);

        var requestBody = new Dictionary<string, object>
        {
            ["model"] = config.ModelName,
            ["messages"] = new object[]
            {
                new { role = "user", content = "ping" }
            },
            ["max_tokens"] = 1
        };

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            if (!string.IsNullOrWhiteSpace(config.ApiKey))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.ApiKey);
            }

            if (ProviderName.Equals("openrouter", StringComparison.OrdinalIgnoreCase))
            {
                request.Headers.TryAddWithoutValidation("HTTP-Referer", "https://agentstudio.lab");
                request.Headers.TryAddWithoutValidation("X-Title", "Agent Studio");
            }

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
                401 => Result<bool>.Failure(401, $"API Key không hợp lệ hoặc không có quyền truy cập: {detailedMessage}"),
                404 => Result<bool>.Failure(404, $"Không tìm thấy Model '{config.ModelName}' hoặc endpoint không tồn tại: {detailedMessage}"),
                429 => Result<bool>.Failure(429, $"Tài khoản hết hạn ngạch (quota) hoặc bị giới hạn tốc độ: {detailedMessage}"),
                _ => Result<bool>.Failure((int)response.StatusCode, $"Xác thực {ProviderName} thất bại ({response.StatusCode}): {detailedMessage}")
            };
        }
        catch (HttpRequestException ex)
        {
            return Result<bool>.Failure(502, $"Không thể kết nối đến máy chủ {ProviderName}: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return Result<bool>.Failure(408, $"Quá thời gian kết nối (Timeout) đến {ProviderName}. Vui lòng kiểm tra lại Base URL và kết nối mạng.");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(500, $"Lỗi không xác định khi kiểm tra kết nối: {ex.Message}");
        }
    }

    private bool IsLocalProvider()
    {
        var normalized = ProviderName.Trim().ToLowerInvariant();
        return normalized is "ollama" or "local" or "custom" or "customopenai";
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

    private string ResolveEndpoint(ModelConfig config)
    {
        if (!string.IsNullOrWhiteSpace(config.BaseUrl))
        {
            var url = config.BaseUrl.TrimEnd('/');
            return url.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase)
                ? url
                : $"{url}/chat/completions";
        }

        var normalized = ProviderName.Trim().ToLowerInvariant();

        return normalized switch
        {
            "deepseek" => "https://api.deepseek.com/chat/completions",
            "groq" => "https://api.groq.com/openai/v1/chat/completions",
            "openrouter" => "https://openrouter.ai/api/v1/chat/completions",
            "mistral" => "https://api.mistral.ai/v1/chat/completions",
            "ollama" => "http://localhost:11434/v1/chat/completions",
            "xai" or "grok" => "https://api.x.ai/v1/chat/completions",
            "together" or "togetherai" => "https://api.together.xyz/v1/chat/completions",
            "perplexity" => "https://api.perplexity.ai/chat/completions",
            "fireworks" => "https://api.fireworks.ai/inference/v1/chat/completions",
            _ => "https://api.openai.com/v1/chat/completions"
        };
    }
}
