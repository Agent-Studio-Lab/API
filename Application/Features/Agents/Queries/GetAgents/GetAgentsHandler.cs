using API.Application.Abstractions;
using API.Application.Messaging;
using API.Domain.Common;

namespace API.Application.Features.Agents.Queries.GetAgents;

public sealed class GetAgentsHandler : IQueryHandler<GetAgentsQuery, IReadOnlyList<AgentResponse>>
{
    private readonly IAgentRepository _agentRepository;

    public GetAgentsHandler(IAgentRepository agentRepository)
    {
        _agentRepository = agentRepository;
    }

    public async Task<Result<IReadOnlyList<AgentResponse>>> Handle(GetAgentsQuery request, CancellationToken cancellationToken)
    {
        var agents = await _agentRepository.GetAllAsync(cancellationToken);

        var responses = agents.Select(a => new AgentResponse(
            a.Id,
            a.Name,
            a.Avatar,
            a.RoleTitle,
            a.Department,
            a.EmploymentType.ToString(),
            a.Skills,
            a.ModelConfig.Provider.ToString(),
            a.ModelConfig.ModelName,
            a.Status
        )).ToList();

        return Result<IReadOnlyList<AgentResponse>>.Success(responses);
    }
}
