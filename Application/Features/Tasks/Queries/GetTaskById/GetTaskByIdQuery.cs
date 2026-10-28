using API.Application.Messaging;

namespace API.Application.Features.Tasks.Queries.GetTaskById;

public sealed record GetTaskByIdQuery(Guid Id) : IQuery<TaskDetailsResponse>;
