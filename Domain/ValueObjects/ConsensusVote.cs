namespace API.Domain.ValueObjects;

public sealed class ConsensusVote
{
    public Guid AgentId { get; }
    public string AgentName { get; }
    public string ProposedPrimaryExecutor { get; }
    public string ProposedTester { get; }
    public bool NeedsHiring { get; }
    public string? MissingRole { get; }
    public string? MissingSkill { get; }
    public string Reasoning { get; }

    public ConsensusVote(
        Guid agentId,
        string agentName,
        string proposedPrimaryExecutor,
        string proposedTester,
        bool needsHiring,
        string? missingRole,
        string? missingSkill,
        string reasoning)
    {
        AgentId = agentId;
        AgentName = agentName;
        ProposedPrimaryExecutor = proposedPrimaryExecutor;
        ProposedTester = proposedTester;
        NeedsHiring = needsHiring;
        MissingRole = missingRole;
        MissingSkill = missingSkill;
        Reasoning = reasoning;
    }
}
