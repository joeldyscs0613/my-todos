using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTodos.BuildingBlocks.Infrastructure.Messaging.Outbox;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Constants;

namespace MyTodos.BuildingBlocks.Infrastructure.Persistence.Configs;

/// <summary>
/// Entity Framework Core configuration for OutboxMessage entity.
/// </summary>
public class OutboxMessageConfig : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        //TODO: should we have a specific schema for it e.g. messaging
        builder.ToTable(TableNames.OutboxMessage);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Type)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Content)
            .IsRequired();

        builder.Property(e => e.OccurredOn)
            .IsRequired();

        builder.Property(e => e.ProcessedOn);

        builder.Property(e => e.Error);

        builder.Property(e => e.RetryCount)
            .IsRequired()
            .HasDefaultValue(0);

        // Index for efficient queries of unprocessed messages
        builder.HasIndex(e => e.ProcessedOn)
            .HasDatabaseName("IX_OutboxMessages_ProcessedOn");

        // Index for ordering by occurred time
        builder.HasIndex(e => e.OccurredOn)
            .HasDatabaseName("IX_OutboxMessages_OccurredOn");
    }
}
