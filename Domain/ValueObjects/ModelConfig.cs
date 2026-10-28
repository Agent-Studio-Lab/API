namespace API.Domain.ValueObjects;

public sealed record ModelConfig(
    string Provider,
    string ModelName,
    string? ApiKey = null,
    string? BaseUrl = null,
    double? Temperature = null);
