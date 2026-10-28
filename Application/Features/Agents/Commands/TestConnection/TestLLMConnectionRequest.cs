namespace API.Application.Features.Agents.Commands.TestConnection;

public sealed record TestLLMConnectionRequest(
    string Provider,
    string ModelName,
    string? ApiKey = null,
    string? BaseUrl = null);
