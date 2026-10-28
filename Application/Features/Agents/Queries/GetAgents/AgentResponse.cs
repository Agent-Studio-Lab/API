namespace API.Application.Features.Agents.Queries.GetAgents;

public sealed record AgentResponse(
    Guid Id,
    string Name,
    string Avatar,
    string RoleTitle,
    string Department,
    string EmploymentType,
    IReadOnlyCollection<string> Skills,
    string Provider,
    string ModelName,
    string Status);
