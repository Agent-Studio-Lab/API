using API.Application.Abstractions;
using API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Infrastructure.Persistence.Repositories;

public sealed class TaskTicketRepository : ITaskTicketRepository
{
    private readonly AgentStudioDbContext _dbContext;

    public TaskTicketRepository(AgentStudioDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(TaskTicket ticket, CancellationToken cancellationToken = default)
    {
        await _dbContext.TaskTickets.AddAsync(ticket, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<TaskTicket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TaskTickets
            .Include(x => x.DebateHistory)
            .Include(x => x.ExecutionHistory)
            .Include(x => x.Deliverables)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(TaskTicket ticket, CancellationToken cancellationToken = default)
    {
        _dbContext.TaskTickets.Update(ticket);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
