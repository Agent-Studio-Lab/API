using API.Application.Features.Tasks.Queries.GetTaskById;
using API.Domain.Entities;

namespace API.Application.Features.Tasks.Mappers;

public static class TaskResponseMapper
{
    public static TaskDetailsResponse ToDetailsResponse(this TaskTicket ticket)
    {
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

        return new TaskDetailsResponse(
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
    }
}
