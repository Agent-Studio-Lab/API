using API.Application.Abstractions;
using Hangfire;

namespace API.Infrastructure.BackgroundJobs;

public sealed class HangfireTaskWorkflowQueue : ITaskWorkflowQueue
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public HangfireTaskWorkflowQueue(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public void EnqueueStep(Guid ticketId)
    {
        _backgroundJobClient.Enqueue<TaskWorkflowJob>(job => job.ProcessTaskStepAsync(ticketId, CancellationToken.None));
    }
}
