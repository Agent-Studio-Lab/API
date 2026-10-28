using System.Text.Json;
using API.Domain.Entities;
using API.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Infrastructure.Persistence.Configurations;

public sealed class AgentConfiguration : IEntityTypeConfiguration<Agent>
{
    public void Configure(EntityTypeBuilder<Agent> builder)
    {
        builder.ToTable("Agents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Avatar)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.RoleTitle)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Department)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.SystemPrompt)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasMaxLength(200);

        builder.Property(x => x.EmploymentType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        // Lưu trữ Skills dưới dạng JSONB
        builder.Property(x => x.Skills)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

        // Map Value Object ModelConfig thành cột JSONB
        builder.OwnsOne(x => x.ModelConfig, cfg =>
        {
            cfg.ToJson();
        });

        // Ignore Memory runtime collection khỏi bảng Agent chính
        builder.Ignore(x => x.Memory);

        // Global query filter cho Soft Delete
        builder.HasQueryFilter(x => !x.IsSoftDeleted);
    }
}
