using API.Domain.Enums;

namespace API.Domain.ValueObjects;

public sealed record ModelConfig(
    LLMProviderType Provider,
    string ModelName,
    string? ApiKey = null,
    string? BaseUrl = null,
    double Temperature = 0.7);
