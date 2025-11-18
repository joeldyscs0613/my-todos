using Microsoft.EntityFrameworkCore;
using MyTodos.BuildingBlocks.Infrastructure.Messaging.Outbox;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Configs;
using MyTodos.SharedKernel.Contracts;

namespace MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions;

/// <summary>
/// Base DbContext for all microservices. Provides OutboxMessages table for reliable event publishing.
/// Services must inherit from this and call base.OnModelCreating().
/// </summary>
public abstract class BaseDbContext : DbContext
{
    /// <summary>
    /// Outbox messages for reliable integration event publishing.
    /// </summary>
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected BaseDbContext(DbContextOptions options) : base(options)
    {
    }

    /// <summary>
    /// Configures outbox table and ignores domain events on aggregates.
    /// Override in derived classes and call base.OnModelCreating() first.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply outbox configuration
        modelBuilder.ApplyConfiguration(new OutboxMessageConfig());

        // Apply common configurations
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Ignore domain events collection
            if (typeof(IAggregateRoot).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Ignore(nameof(IAggregateRoot.DomainEvents));
            }
        }
    }
}
