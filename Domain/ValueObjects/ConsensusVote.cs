namespace API.Domain.ValueObjects;

public sealed record ConsensusVote(
    Guid AgentId,
    string AgentName,
    string ProposedPrimaryExecutor,
    string ProposedTester,
    bool NeedsHiring,
    string? MissingRole,
    string? MissingSkill,
    string Reasoning);
