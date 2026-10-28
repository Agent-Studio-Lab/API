using API.Domain.Common;
using API.Domain.Enums;

namespace API.Domain.Entities;

public sealed class OfficeMessage : Entity
{
    public string SenderName { get; private set; }
    public string? ReceiverName { get; private set; }
    public string Content { get; private set; }
    public TaskPhase Phase { get; private set; }
    public DateTime Timestamp { get; private set; }

    private OfficeMessage() : base(null)
    {
        SenderName = string.Empty;
        Content = string.Empty;
    }

    public OfficeMessage(
        string senderName,
        string? receiverName,
        string content,
        TaskPhase phase,
        DateTime timestamp,
        Guid? id = null) : base(id)
    {
        SenderName = senderName;
        ReceiverName = receiverName;
        Content = content;
        Phase = phase;
        Timestamp = timestamp;
    }
}
