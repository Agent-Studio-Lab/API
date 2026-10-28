using API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Infrastructure.Persistence.Configurations;

public sealed class DebateTurnConfiguration : IEntityTypeConfiguration<DebateTurn>
{
    public void Configure(EntityTypeBuilder<DebateTurn> builder)
    {
        builder.ToTable("debate_turns");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AgentName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.AgentAvatar)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Statement)
            .IsRequired();

        builder.HasQueryFilter(x => !x.IsSoftDeleted);
    }
}
