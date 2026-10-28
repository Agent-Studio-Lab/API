namespace API.Domain.Constants;

public static class LLMProviders
{
    public const string OpenAI = "OpenAI";
    public const string Gemini = "Gemini";
    public const string Anthropic = "Anthropic";
    public const string DeepSeek = "DeepSeek";
    public const string Groq = "Groq";
    public const string OpenRouter = "OpenRouter";
    public const string Mistral = "Mistral";
    public const string Ollama = "Ollama";
    public const string XAI = "xAI";
    public const string Perplexity = "Perplexity";
    public const string Together = "Together";
    public const string Fireworks = "Fireworks";
    public const string Custom = "Custom";
    public const string Mock = "Mock";

    private static readonly HashSet<string> KnownSet = new(StringComparer.OrdinalIgnoreCase)
    {
        OpenAI, Gemini, "Google", Anthropic, "Claude", DeepSeek, Groq, OpenRouter, Mistral, Ollama, XAI, "Grok", Perplexity, Together, "TogetherAI", Fireworks, Custom, "CustomOpenAI", Mock
    };

    public static bool IsKnownProvider(string? provider)
    {
        if (string.IsNullOrWhiteSpace(provider)) return false;
        return KnownSet.Contains(provider.Trim());
    }

    public static bool IsLocalOrMock(string? provider)
    {
        if (string.IsNullOrWhiteSpace(provider)) return true;
        var p = provider.Trim().ToLowerInvariant();
        return p is "mock" or "ollama" or "local" or "custom" or "customopenai";
    }
}
