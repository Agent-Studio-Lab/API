using API.Domain.Common;

namespace API.Domain.Entities;

public sealed class DebateTurn : Entity
{
    public Guid TaskTicketId { get; private set; }
    public Guid AgentId { get; private set; }
    public string AgentName { get; private set; }
    public string? AgentAvatar { get; private set; }
    public int RoundNumber { get; private set; }
    public string Statement { get; private set; }

    private DebateTurn() : base(null)
    {
        AgentName = string.Empty;
        Statement = string.Empty;
    }

    public DebateTurn(
        Guid taskTicketId,
        Guid agentId,
        string agentName,
        string? agentAvatar,
        int roundNumber,
        string statement,
        Guid? id = null) : base(id)
    {
        TaskTicketId = taskTicketId;
        AgentId = agentId;
        AgentName = agentName;
        AgentAvatar = agentAvatar;
        RoundNumber = roundNumber;
        Statement = statement;
    }
}
