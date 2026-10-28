using System.Text.Json;
using API.Domain.Entities;
using API.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Infrastructure.Persistence.Configurations;

public sealed class TaskTicketConfiguration : IEntityTypeConfiguration<TaskTicket>
{
    public void Configure(EntityTypeBuilder<TaskTicket> builder)
    {
        builder.ToTable("TaskTickets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.Description)
            .IsRequired();

        builder.Property(x => x.CurrentPhase)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.ExecutionMode)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.MaxFeedbackLoops)
            .IsRequired();

        builder.Property(x => x.CurrentFeedbackLoop)
            .IsRequired();

        // Map Value Object FinalPlan thành cột JSONB
        builder.Property(x => x.FinalPlan)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<ConsensusPlan>(v, (JsonSerializerOptions?)null));

        // Map Value Object PendingHiring thành cột JSONB
        builder.Property(x => x.PendingHiring)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<HiringProposal>(v, (JsonSerializerOptions?)null));

        // Map danh sách Votes thành JSONB
        builder.Property(x => x.Votes)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<ConsensusVote>>(v, (JsonSerializerOptions?)null) ?? new List<ConsensusVote>());

        // Quan hệ 1-N với DebateTurn
        builder.HasMany(x => x.DebateHistory)
            .WithOne()
            .HasForeignKey(x => x.TaskTicketId)
            .OnDelete(DeleteBehavior.Cascade);

        // Quan hệ 1-N với ExecutionTurn
        builder.HasMany(x => x.ExecutionHistory)
            .WithOne()
            .HasForeignKey(x => x.TaskTicketId)
            .OnDelete(DeleteBehavior.Cascade);

        // Quan hệ 1-N với TaskDeliverable
        builder.HasMany(x => x.Deliverables)
            .WithOne()
            .HasForeignKey(x => x.TaskTicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.IsSoftDeleted);
    }
}
