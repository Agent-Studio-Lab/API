using API.Application.Abstractions;
using API.Application.Features.Tasks.Mappers;
using API.Application.Messaging;
using API.Domain.Common;

namespace API.Application.Features.Tasks.Queries.GetTaskById;

public sealed class GetTaskByIdHandler : IQueryHandler<GetTaskByIdQuery, TaskDetailsResponse>
{
    private readonly ITaskTicketRepository _taskTicketRepository;

    public GetTaskByIdHandler(ITaskTicketRepository taskTicketRepository)
    {
        _taskTicketRepository = taskTicketRepository;
    }

    public async Task<Result<TaskDetailsResponse>> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _taskTicketRepository.GetByIdAsync(request.Id, cancellationToken);
        if (ticket is null)
        {
            return Result<TaskDetailsResponse>.Failure(404, $"Không tìm thấy nhiệm vụ với Id: {request.Id}");
        }

        return Result<TaskDetailsResponse>.Success(ticket.ToDetailsResponse());
    }
}
