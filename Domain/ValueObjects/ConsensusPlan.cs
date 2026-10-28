namespace API.Domain.ValueObjects;

public sealed class ConsensusPlan
{
    public Guid PrimaryExecutorId { get; }
    public string PrimaryExecutorName { get; }
    public Guid TesterId { get; }
    public string TesterName { get; }
    public string ExecutionPlanSummary { get; }
    public bool RequiresHiring { get; }
    public HiringProposal? ProposedHire { get; }

    public ConsensusPlan(
        Guid primaryExecutorId,
        string primaryExecutorName,
        Guid testerId,
        string testerName,
        string executionPlanSummary,
        bool requiresHiring,
        HiringProposal? proposedHire = null)
    {
        PrimaryExecutorId = primaryExecutorId;
        PrimaryExecutorName = primaryExecutorName;
        TesterId = testerId;
        TesterName = testerName;
        ExecutionPlanSummary = executionPlanSummary;
        RequiresHiring = requiresHiring;
        ProposedHire = proposedHire;
    }
}
