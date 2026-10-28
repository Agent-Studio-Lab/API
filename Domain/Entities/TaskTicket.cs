using API.Domain.Common;
using API.Domain.Enums;
using API.Domain.ValueObjects;

namespace API.Domain.Entities;

public sealed class TaskTicket : AggregateRoot
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public TaskPhase CurrentPhase { get; private set; }
    public TaskExecutionMode ExecutionMode { get; private set; }
    public int MaxFeedbackLoops { get; private set; }
    public int CurrentFeedbackLoop { get; private set; }

    public ConsensusPlan? FinalPlan { get; private set; }
    public HiringProposal? PendingHiring { get; private set; }
    public Guid? RecruitedAgentId { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private readonly List<DebateTurn> _debateHistory = [];
    public IReadOnlyCollection<DebateTurn> DebateHistory => _debateHistory.AsReadOnly();

    private readonly List<ConsensusVote> _votes = [];
    public IReadOnlyCollection<ConsensusVote> Votes => _votes.AsReadOnly();

    private readonly List<ExecutionTurn> _executionHistory = [];
    public IReadOnlyCollection<ExecutionTurn> ExecutionHistory => _executionHistory.AsReadOnly();

    private readonly List<TaskDeliverable> _deliverables = [];
    public IReadOnlyCollection<TaskDeliverable> Deliverables => _deliverables.AsReadOnly();

    public TaskTicket(
        string title,
        string description,
        TaskExecutionMode executionMode,
        int maxFeedbackLoops,
        Guid? id = null) : base(id)
    {
        Title = title;
        Description = description;
        CurrentPhase = TaskPhase.Draft;
        ExecutionMode = executionMode;
        MaxFeedbackLoops = maxFeedbackLoops;
        CurrentFeedbackLoop = 0;
    }

    public void StartDebate()
    {
        CurrentPhase = TaskPhase.Debating;
    }

    public void AddDebateTurn(DebateTurn turn)
    {
        _debateHistory.Add(turn);
    }

    public void AddVote(ConsensusVote vote)
    {
        _votes.Add(vote);
    }

    public void SetConsensusPlan(ConsensusPlan plan)
    {
        FinalPlan = plan;
        if (plan.RequiresHiring && plan.ProposedHire is not null)
        {
            PendingHiring = plan.ProposedHire;
            CurrentPhase = TaskPhase.WaitingHiringApproval;
        }
        else
        {
            CurrentPhase = TaskPhase.Executing;
        }
    }

    public void ApproveHiring(Guid recruitedAgentId)
    {
        RecruitedAgentId = recruitedAgentId;
        PendingHiring = null;
        CurrentPhase = TaskPhase.Executing;
    }

    public void RejectHiring()
    {
        PendingHiring = null;
        CurrentPhase = TaskPhase.Executing;
    }

    public void StartTesting()
    {
        CurrentPhase = TaskPhase.TestingAndReviewing;
    }

    public void RecordExecutionTurn(ExecutionTurn turn)
    {
        _executionHistory.Add(turn);
        if (turn.IsIssueFound)
        {
            CurrentFeedbackLoop++;
            CurrentPhase = TaskPhase.Executing;
        }
    }

    public void AddDeliverable(TaskDeliverable deliverable)
    {
        _deliverables.Add(deliverable);
    }

    public void MarkCompleted()
    {
        CurrentPhase = TaskPhase.Completed;
        CompletedAt = Clock.Now;
    }

    public void MarkWaitingFinalApproval()
    {
        CurrentPhase = TaskPhase.WaitingFinalApproval;
    }

    public void MarkFailed()
    {
        CurrentPhase = TaskPhase.Failed;
    }
}
