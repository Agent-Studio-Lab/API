using API.Domain.Enums;

namespace API.Application.Features.Agents.Commands.TestConnection;

public sealed record TestLLMConnectionRequest(
    LLMProviderType Provider,
    string ModelName,
    string? ApiKey = null,
    string? BaseUrl = null);
