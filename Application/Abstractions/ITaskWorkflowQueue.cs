namespace API.Application.Abstractions;

public interface ITaskWorkflowQueue
{
    void EnqueueStep(Guid ticketId);
}
