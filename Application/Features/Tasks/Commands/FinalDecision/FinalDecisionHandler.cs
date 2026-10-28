using API.Application.Abstractions;
using API.Application.Messaging;
using API.Domain.Common;
using API.Domain.Enums;

namespace API.Application.Features.Tasks.Commands.FinalDecision;

public sealed class FinalDecisionHandler : ICommandHandler<FinalDecisionCommand, bool>
{
    private readonly ITaskTicketRepository _taskTicketRepository;
    private readonly IAgentRepository _agentRepository;

    public FinalDecisionHandler(
        ITaskTicketRepository taskTicketRepository,
        IAgentRepository agentRepository)
    {
        _taskTicketRepository = taskTicketRepository;
        _agentRepository = agentRepository;
    }

    public async Task<Result<bool>> Handle(FinalDecisionCommand command, CancellationToken cancellationToken)
    {
        var ticket = await _taskTicketRepository.GetByIdAsync(command.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result<bool>.Failure(404, "Không tìm thấy nhiệm vụ.");
        }

        if (ticket.CurrentPhase != TaskPhase.WaitingFinalApproval)
        {
            return Result<bool>.Failure(400, "Nhiệm vụ chưa ở giai đoạn chờ nghiệm thu cuối cùng.");
        }

        // Xử lý nhân sự dự án nếu có
        if (ticket.RecruitedAgentId.HasValue)
        {
            var agent = await _agentRepository.GetByIdAsync(ticket.RecruitedAgentId.Value, cancellationToken);
            if (agent is not null)
            {
                if (command.Request.RetainAgentAsPermanent)
                {
                    agent.PromoteToPermanent();
                    await _agentRepository.UpdateAsync(agent, cancellationToken);
                }
                else
                {
                    agent.SetDeleted(Clock.Now, "System");
                    await _agentRepository.UpdateAsync(agent, cancellationToken);
                }
            }
        }

        ticket.MarkCompleted();
        await _taskTicketRepository.UpdateAsync(ticket, cancellationToken);

        return Result<bool>.Success(true);
    }
}
