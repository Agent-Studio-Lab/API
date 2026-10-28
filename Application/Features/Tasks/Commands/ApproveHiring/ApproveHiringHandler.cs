using API.Application.Abstractions;
using API.Application.Messaging;
using API.Domain.Common;
using API.Domain.Entities;
using API.Domain.Enums;
using API.Domain.ValueObjects;

namespace API.Application.Features.Tasks.Commands.ApproveHiring;

public sealed class ApproveHiringHandler : ICommandHandler<ApproveHiringCommand, bool>
{
    private readonly ITaskTicketRepository _taskTicketRepository;
    private readonly IAgentRepository _agentRepository;
    private readonly ITaskWorkflowQueue _workflowQueue;

    public ApproveHiringHandler(
        ITaskTicketRepository taskTicketRepository,
        IAgentRepository agentRepository,
        ITaskWorkflowQueue workflowQueue)
    {
        _taskTicketRepository = taskTicketRepository;
        _agentRepository = agentRepository;
        _workflowQueue = workflowQueue;
    }

    public async Task<Result<bool>> Handle(ApproveHiringCommand command, CancellationToken cancellationToken)
    {
        var ticket = await _taskTicketRepository.GetByIdAsync(command.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result<bool>.Failure(404, "Không tìm thấy nhiệm vụ.");
        }

        if (ticket.CurrentPhase != TaskPhase.WaitingHiringApproval || ticket.PendingHiring is null)
        {
            return Result<bool>.Failure(400, "Nhiệm vụ không ở trạng thái chờ duyệt tuyển dụng.");
        }

        var req = command.Request;
        var proposal = ticket.PendingHiring;
        var modelConfig = new ModelConfig(req.Provider, req.ModelName, req.ApiKey, req.BaseUrl);

        // Sử dụng Domain Factory Method
        var newAgent = Agent.CreateContractor(req.Name, req.Avatar, proposal, modelConfig);
        await _agentRepository.AddAsync(newAgent, cancellationToken);

        ticket.ApproveHiring(newAgent.Id);
        await _taskTicketRepository.UpdateAsync(ticket, cancellationToken);

        // Kích hoạt qua Application Abstraction (không phụ thuộc Hangfire)
        _workflowQueue.EnqueueStep(ticket.Id);

        return Result<bool>.Success(true);
    }
}
