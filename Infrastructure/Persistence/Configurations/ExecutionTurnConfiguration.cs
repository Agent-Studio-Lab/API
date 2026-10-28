using API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Infrastructure.Persistence.Configurations;

public sealed class ExecutionTurnConfiguration : IEntityTypeConfiguration<ExecutionTurn>
{
    public void Configure(EntityTypeBuilder<ExecutionTurn> builder)
    {
        builder.ToTable("ExecutionTurns");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SpeakerName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.SpeakerRoleTitle)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.IssueDetails);

        builder.HasQueryFilter(x => !x.IsSoftDeleted);
    }
}
