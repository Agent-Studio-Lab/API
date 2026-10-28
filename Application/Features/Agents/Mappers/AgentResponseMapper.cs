using API.Application.Features.Agents.Queries.GetAgents;
using API.Domain.Entities;

namespace API.Application.Features.Agents.Mappers;

public static class AgentResponseMapper
{
    public static AgentResponse ToResponse(this Agent agent)
    {
        return new AgentResponse(
            agent.Id,
            agent.Name,
            agent.Avatar,
            agent.RoleTitle,
            agent.Department,
            agent.EmploymentType.ToString(),
            agent.Skills,
            agent.ModelConfig.Provider.ToString(),
            agent.ModelConfig.ModelName,
            agent.Status);
    }

    public static IReadOnlyList<AgentResponse> ToResponse(this IEnumerable<Agent> agents)
    {
        return agents.Select(ToResponse).ToList();
    }
}
