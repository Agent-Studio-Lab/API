using API.Application.Messaging;

namespace API.Application.Features.Tasks.Commands.ExecuteTask;

public sealed record ExecuteTaskCommand(ExecuteTaskRequest Request) : ICommand<ExecuteTaskResponse>;
