using API.Application.Abstractions;
using API.Domain.Enums;
using API.Domain.ValueObjects;

namespace API.Infrastructure.LLM;

public sealed class LLMProviderFactory : ILLMProviderFactory
{
    private readonly IHttpClientFactory _httpClientFactory;

    public LLMProviderFactory(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public ILLMProvider GetProvider(LLMProviderType providerType)
    {
        var httpClient = _httpClientFactory.CreateClient();

        return providerType switch
        {
            LLMProviderType.Gemini => new GeminiLLMProvider(httpClient),
            LLMProviderType.OpenAI => new OpenAILikeLLMProvider(httpClient, LLMProviderType.OpenAI),
            LLMProviderType.DeepSeek => new OpenAILikeLLMProvider(httpClient, LLMProviderType.DeepSeek),
            LLMProviderType.Ollama => new OpenAILikeLLMProvider(httpClient, LLMProviderType.Ollama),
            _ => new MockLLMProvider(providerType)
        };
    }
}
