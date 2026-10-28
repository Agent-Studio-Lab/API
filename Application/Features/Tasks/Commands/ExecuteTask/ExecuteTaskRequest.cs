using API.Domain.Enums;

namespace API.Application.Features.Tasks.Commands.ExecuteTask;

public sealed record ExecuteTaskRequest(
    string Prompt,
    TaskExecutionMode ExecutionMode = TaskExecutionMode.LimitedLoops,
    int MaxFeedbackLoops = 3);
