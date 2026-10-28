using API.Domain.Entities;

namespace API.Application.Abstractions;

public interface IAgentRepository
{
    Task<IReadOnlyList<Agent>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Agent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Agent>> GetByDepartmentAsync(string department, CancellationToken cancellationToken = default);
    Task AddAsync(Agent agent, CancellationToken cancellationToken = default);
    Task UpdateAsync(Agent agent, CancellationToken cancellationToken = default);
}
