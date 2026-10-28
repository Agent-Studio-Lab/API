using API.Domain.Enums;

namespace API.Application.Features.Tasks.Commands.ApproveHiring;

public sealed record ApproveHiringRequest(
    string Name,
    string? Avatar,
    LLMProviderType Provider,
    string ModelName,
    string ApiKey,
    string? BaseUrl = null);
