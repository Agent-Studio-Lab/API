namespace API.Application.Features.Tasks.Commands.ExecuteTask;

public sealed record ExecuteTaskResponse(
    Guid TicketId,
    string Title,
    string Status,
    string Message);
