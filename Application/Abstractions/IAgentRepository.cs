using API.Domain.Entities;
using API.Domain.Enums;

namespace API.Application.Abstractions;

public interface IAgentRepository
{
    Task<IReadOnlyList<Agent>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Agent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Agent?> GetByRoleAsync(AgentRole role, CancellationToken cancellationToken = default);
    Task UpdateAsync(Agent agent, CancellationToken cancellationToken = default);
}
