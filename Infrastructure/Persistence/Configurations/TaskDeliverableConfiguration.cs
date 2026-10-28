using API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Infrastructure.Persistence.Configurations;

public sealed class TaskDeliverableConfiguration : IEntityTypeConfiguration<TaskDeliverable>
{
    public void Configure(EntityTypeBuilder<TaskDeliverable> builder)
    {
        builder.ToTable("TaskDeliverables");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Content)
            .IsRequired();

        builder.HasQueryFilter(x => !x.IsSoftDeleted);
    }
}
