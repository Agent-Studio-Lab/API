using API.Application.Abstractions;

namespace API.Infrastructure.LLM;

public sealed class LLMProviderFactory : ILLMProviderFactory
{
    private readonly IHttpClientFactory _httpClientFactory;

    public LLMProviderFactory(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public ILLMProvider GetProvider(string providerName)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var safeName = string.IsNullOrWhiteSpace(providerName) ? "Mock" : providerName.Trim();
        var normalized = safeName.ToLowerInvariant();

        if (normalized == "mock")
        {
            return new MockLLMProvider(safeName);
        }

        if (normalized.Contains("gemini") || normalized.Contains("google"))
        {
            return new GeminiLLMProvider(httpClient, safeName);
        }

        if (normalized.Contains("anthropic") || normalized.Contains("claude"))
        {
            return new AnthropicLLMProvider(httpClient, safeName);
        }

        // All other providers (OpenAI, DeepSeek, Groq, OpenRouter, Mistral, Ollama, xAI, Together, Perplexity, Fireworks, Cerebras, Custom, etc.)
        // use standard OpenAI-compatible completions protocol
        return new OpenAILikeLLMProvider(httpClient, safeName);
    }
}
