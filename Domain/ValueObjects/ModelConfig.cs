using API.Domain.Enums;

namespace API.Domain.ValueObjects;

public sealed class ModelConfig
{
    public LLMProviderType Provider { get; }
    public string ModelName { get; }
    public string? ApiKey { get; }
    public string? BaseUrl { get; }
    public double Temperature { get; }

    public ModelConfig(
        LLMProviderType provider,
        string modelName,
        string? apiKey = null,
        string? baseUrl = null,
        double temperature = 0.7)
    {
        Provider = provider;
        ModelName = modelName;
        ApiKey = apiKey;
        BaseUrl = baseUrl;
        Temperature = temperature;
    }
}
