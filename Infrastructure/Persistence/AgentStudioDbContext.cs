using System.Reflection;
using API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Infrastructure.Persistence;

public sealed class AgentStudioDbContext : BaseDbContext
{
    public AgentStudioDbContext(DbContextOptions<AgentStudioDbContext> options) : base(options)
    {
    }

    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<TaskTicket> TaskTickets => Set<TaskTicket>();
    public DbSet<DebateTurn> DebateTurns => Set<DebateTurn>();
    public DbSet<ExecutionTurn> ExecutionTurns => Set<ExecutionTurn>();
    public DbSet<TaskDeliverable> TaskDeliverables => Set<TaskDeliverable>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Enum>()
            .HaveConversion<string>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
