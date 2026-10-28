using API.Application.Messaging;

namespace API.Application.Features.Agents.Queries.GetAgents;

public sealed record GetAgentsQuery : IQuery<IReadOnlyList<AgentResponse>>;
