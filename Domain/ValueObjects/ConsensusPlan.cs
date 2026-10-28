namespace API.Domain.ValueObjects;

public sealed record ConsensusPlan(
    Guid PrimaryExecutorId,
    string PrimaryExecutorName,
    Guid TesterId,
    string TesterName,
    string ExecutionPlanSummary,
    bool RequiresHiring,
    HiringProposal? ProposedHire = null);
