using API.Application.Abstractions;
using API.Application.Messaging;
using API.Domain.Common;
using API.Domain.ValueObjects;

namespace API.Application.Features.Agents.Commands.UpdateConfig;

public sealed class UpdateAgentConfigHandler : ICommandHandler<UpdateAgentConfigCommand, bool>
{
    private readonly IAgentRepository _agentRepository;

    public UpdateAgentConfigHandler(IAgentRepository agentRepository)
    {
        _agentRepository = agentRepository;
    }

    public async Task<Result<bool>> Handle(UpdateAgentConfigCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var agent = await _agentRepository.GetByIdAsync(request.AgentId, cancellationToken);

        if (agent is null)
        {
            return Result<bool>.Failure(404, $"Không tìm thấy nhân viên với Id: {request.AgentId}");
        }

        var newConfig = new ModelConfig(
            request.Provider,
            request.ModelName,
            request.ApiKey,
            request.BaseUrl);

        agent.UpdateModelConfig(newConfig);
        await _agentRepository.UpdateAsync(agent, cancellationToken);

        return Result<bool>.Success(true);
    }
}
