using API.Domain.Common;

namespace API.Domain.Entities;

public sealed class ExecutionTurn : Entity
{
    public Guid TaskTicketId { get; private set; }
    public int LoopNumber { get; private set; }
    public Guid SpeakerAgentId { get; private set; }
    public string SpeakerName { get; private set; }
    public string SpeakerRoleTitle { get; private set; }
    public string Content { get; private set; }
    public bool IsIssueFound { get; private set; }
    public string? IssueDetails { get; private set; }

    private ExecutionTurn() : base(null)
    {
        SpeakerName = string.Empty;
        SpeakerRoleTitle = string.Empty;
        Content = string.Empty;
    }

    public ExecutionTurn(
        Guid taskTicketId,
        int loopNumber,
        Guid speakerAgentId,
        string speakerName,
        string speakerRoleTitle,
        string content,
        bool isIssueFound,
        string? issueDetails,
        Guid? id = null) : base(id)
    {
        TaskTicketId = taskTicketId;
        LoopNumber = loopNumber;
        SpeakerAgentId = speakerAgentId;
        SpeakerName = speakerName;
        SpeakerRoleTitle = speakerRoleTitle;
        Content = content;
        IsIssueFound = isIssueFound;
        IssueDetails = issueDetails;
    }
}
