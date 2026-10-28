using API.Domain.Common;

namespace API.Domain.Entities;

public sealed class TaskDeliverable : Entity
{
    public Guid TaskTicketId { get; private set; }
    public string Title { get; private set; }
    public string ContentType { get; private set; }
    public string Content { get; private set; }
    public Guid CreatedByAgentId { get; private set; }

    public TaskDeliverable(
        Guid taskTicketId,
        string title,
        string contentType,
        string content,
        Guid createdByAgentId,
        string createdByAgentName,
        Guid? id = null) : base(id)
    {
        TaskTicketId = taskTicketId;
        Title = title;
        ContentType = contentType;
        Content = content;
        CreatedByAgentId = createdByAgentId;
        SetCreated(Clock.Now, createdByAgentName);
    }
}
