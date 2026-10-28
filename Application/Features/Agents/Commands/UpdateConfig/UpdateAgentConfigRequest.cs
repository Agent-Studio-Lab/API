using API.Domain.Enums;

namespace API.Application.Features.Agents.Commands.UpdateConfig;

public sealed record UpdateAgentConfigRequest(
    Guid AgentId,
    LLMProviderType Provider,
    string ModelName,
    string? ApiKey = null,
    string? BaseUrl = null,
    bool ValidateConnection = true);
