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
            LLMProviderType.Anthropic => new AnthropicLLMProvider(httpClient),
            LLMProviderType.OpenAI => new OpenAILikeLLMProvider(httpClient, LLMProviderType.OpenAI),
            LLMProviderType.DeepSeek => new OpenAILikeLLMProvider(httpClient, LLMProviderType.DeepSeek),
            LLMProviderType.Groq => new OpenAILikeLLMProvider(httpClient, LLMProviderType.Groq),
            LLMProviderType.OpenRouter => new OpenAILikeLLMProvider(httpClient, LLMProviderType.OpenRouter),
            LLMProviderType.Mistral => new OpenAILikeLLMProvider(httpClient, LLMProviderType.Mistral),
            LLMProviderType.Ollama => new OpenAILikeLLMProvider(httpClient, LLMProviderType.Ollama),
            LLMProviderType.CustomOpenAI => new OpenAILikeLLMProvider(httpClient, LLMProviderType.CustomOpenAI),
            _ => new MockLLMProvider(providerType)
        };
    }
}
