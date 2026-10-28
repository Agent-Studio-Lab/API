namespace API.Application.Features.Agents.Queries.GetAgents;

public sealed record AgentResponse(
    Guid Id,
    string Name,
    string Avatar,
    string Role,
    string RoleTitle,
    string Department,
    string Provider,
    string ModelName,
    string Status);
