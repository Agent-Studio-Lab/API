using API.Application.Abstractions;
using API.Application.Messaging;
using API.Domain.Common;
using API.Domain.Entities;
using API.Domain.Enums;
using API.Domain.ValueObjects;
using API.Infrastructure.BackgroundJobs;
using Hangfire;

namespace API.Application.Features.Tasks.Commands.ApproveHiring;

public sealed class ApproveHiringHandler : ICommandHandler<ApproveHiringCommand, bool>
{
    private readonly ITaskTicketRepository _taskTicketRepository;
    private readonly IAgentRepository _agentRepository;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public ApproveHiringHandler(
        ITaskTicketRepository taskTicketRepository,
        IAgentRepository agentRepository,
        IBackgroundJobClient backgroundJobClient)
    {
        _taskTicketRepository = taskTicketRepository;
        _agentRepository = agentRepository;
        _backgroundJobClient = backgroundJobClient;
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

        var newAgent = new Agent(
            name: req.Name,
            avatar: req.Avatar,
            roleTitle: proposal.RoleTitle,
            department: proposal.Department,
            systemPrompt: $"Bạn là {req.Name}, đảm nhiệm vai trò {proposal.RoleTitle}. Bạn có trách nhiệm giải quyết yêu cầu dự án theo đúng kỹ năng chuyên môn: {proposal.RequiredSkill}.",
            modelConfig: new ModelConfig(req.Provider, req.ModelName, req.ApiKey, req.BaseUrl),
            employmentType: AgentEmploymentType.Contractor,
            skills: new[] { proposal.RequiredSkill });

        await _agentRepository.AddAsync(newAgent, cancellationToken);

        ticket.ApproveHiring(newAgent.Id);
        await _taskTicketRepository.UpdateAsync(ticket, cancellationToken);

        _backgroundJobClient.Enqueue<TaskWorkflowJob>(job => job.ProcessTaskStepAsync(ticket.Id, CancellationToken.None));

        return Result<bool>.Success(true);
    }
}
