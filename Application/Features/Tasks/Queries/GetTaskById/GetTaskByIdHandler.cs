using API.Application.Abstractions;
using API.Application.Messaging;
using API.Domain.Common;

namespace API.Application.Features.Tasks.Queries.GetTaskById;

public sealed class GetTaskByIdHandler : IQueryHandler<GetTaskByIdQuery, TaskDetailsResponse>
{
    private readonly ITaskTicketRepository _taskTicketRepository;

    public GetTaskByIdHandler(ITaskTicketRepository taskTicketRepository)
    {
        _taskTicketRepository = taskTicketRepository;
    }

    public async Task<Result<TaskDetailsResponse>> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _taskTicketRepository.GetByIdAsync(request.Id, cancellationToken);
        if (ticket is null)
        {
            return Result<TaskDetailsResponse>.Failure(404, $"Không tìm thấy nhiệm vụ với Id: {request.Id}");
        }

        var debateResponses = ticket.DebateHistory
            .OrderBy(x => x.CreatedAt)
            .Select(d => new DebateTurnResponse(
                d.Id,
                d.AgentId,
                d.AgentName,
                d.AgentAvatar,
                d.RoundNumber,
                d.Statement,
                d.CreatedAt))
            .ToList();

        var executionResponses = ticket.ExecutionHistory
            .OrderBy(x => x.CreatedAt)
            .Select(e => new ExecutionTurnResponse(
                e.Id,
                e.LoopNumber,
                e.SpeakerAgentId,
                e.SpeakerName,
                e.SpeakerRoleTitle,
                e.Content,
                e.IsIssueFound,
                e.IssueDetails,
                e.CreatedAt))
            .ToList();

        var deliverableResponses = ticket.Deliverables
            .OrderBy(x => x.CreatedAt)
            .Select(del => new DeliverableResponse(
                del.Id,
                del.Title,
                del.ContentType,
                del.Content,
                del.CreatedByAgentId,
                del.CreatedAt))
            .ToList();

        var response = new TaskDetailsResponse(
            ticket.Id,
            ticket.Title,
            ticket.Description,
            ticket.CurrentPhase.ToString(),
            ticket.ExecutionMode.ToString(),
            ticket.MaxFeedbackLoops,
            ticket.CurrentFeedbackLoop,
            ticket.FinalPlan,
            ticket.PendingHiring,
            ticket.RecruitedAgentId,
            ticket.CompletedAt,
            debateResponses,
            ticket.Votes.ToList(),
            executionResponses,
            deliverableResponses);

        return Result<TaskDetailsResponse>.Success(response);
    }
}
