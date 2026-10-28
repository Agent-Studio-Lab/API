using API.Application.Abstractions;
using API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Infrastructure.Persistence.Repositories;

public sealed class AgentRepository : IAgentRepository
{
    private readonly AgentStudioDbContext _dbContext;

    public AgentRepository(AgentStudioDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Agent>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Agents
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Agent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Agents
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Agent>> GetByDepartmentAsync(string department, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Agents
            .AsNoTracking()
            .Where(x => x.Department == department)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Agent agent, CancellationToken cancellationToken = default)
    {
        await _dbContext.Agents.AddAsync(agent, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Agent agent, CancellationToken cancellationToken = default)
    {
        _dbContext.Agents.Update(agent);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
