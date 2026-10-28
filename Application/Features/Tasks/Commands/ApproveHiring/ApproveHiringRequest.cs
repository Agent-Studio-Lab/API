namespace API.Application.Features.Tasks.Commands.ApproveHiring;

public sealed record ApproveHiringRequest(
    string Name,
    string? Avatar,
    string Provider,
    string ModelName,
    string ApiKey,
    string? BaseUrl = null);
