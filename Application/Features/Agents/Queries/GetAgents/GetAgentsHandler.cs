using API.Application.Features.Agents.Mappers;
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
        return Result<IReadOnlyList<AgentResponse>>.Success(agents.ToResponse());
    }
}
