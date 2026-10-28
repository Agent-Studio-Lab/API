using API.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace API.Infrastructure.Persistence;

public abstract class BaseDbContext : DbContext
{
    protected BaseDbContext(DbContextOptions options) : base(options)
    {
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<Entity>();
        const string userId = "system";

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity.CreatedAt == default)
                    {
                        entry.Entity.SetCreated(Clock.Now, userId);
                    }
                    break;

                case EntityState.Modified:
                    if (!entry.Property(nameof(Entity.UpdatedAt)).IsModified &&
                        !entry.Property(nameof(Entity.UpdatedBy)).IsModified)
                    {
                        entry.Entity.SetUpdated(Clock.Now, userId);
                    }
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.SetDeleted(Clock.Now, userId);
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
