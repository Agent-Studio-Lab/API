using API.Domain.Entities;

namespace API.Application.Abstractions;

public interface ITaskTicketRepository
{
    Task AddAsync(TaskTicket ticket, CancellationToken cancellationToken = default);
    Task<TaskTicket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
