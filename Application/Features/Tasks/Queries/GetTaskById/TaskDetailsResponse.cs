using API.Domain.ValueObjects;

namespace API.Application.Features.Tasks.Queries.GetTaskById;

public sealed record TaskDetailsResponse(
    Guid Id,
    string Title,
    string Description,
    string CurrentPhase,
    string ExecutionMode,
    int MaxFeedbackLoops,
    int CurrentFeedbackLoop,
    ConsensusPlan? FinalPlan,
    HiringProposal? PendingHiring,
    Guid? RecruitedAgentId,
    DateTime? CompletedAt,
    IReadOnlyList<DebateTurnResponse> DebateHistory,
    IReadOnlyList<ConsensusVote> Votes,
    IReadOnlyList<ExecutionTurnResponse> ExecutionHistory,
    IReadOnlyList<DeliverableResponse> Deliverables);

public sealed record DebateTurnResponse(
    Guid Id,
    Guid AgentId,
    string AgentName,
    string? AgentAvatar,
    int RoundNumber,
    string Statement,
    DateTime CreatedAt);

public sealed record ExecutionTurnResponse(
    Guid Id,
    int LoopNumber,
    Guid SpeakerAgentId,
    string SpeakerName,
    string SpeakerRoleTitle,
    string Content,
    bool IsIssueFound,
    string? IssueDetails,
    DateTime CreatedAt);

public sealed record DeliverableResponse(
    Guid Id,
    string Title,
    string ContentType,
    string Content,
    Guid CreatedByAgentId,
    DateTime CreatedAt);
