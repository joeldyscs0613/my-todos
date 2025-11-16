using Microsoft.EntityFrameworkCore;
using MyTodos.BuildingBlocks.Application.Contracts;
using MyTodos.BuildingBlocks.Application.Contracts.DomainEvents;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.SharedKernel.Abstractions;
using MyTodos.SharedKernel.Contracts;

namespace MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions;

/// <summary>
/// Base Unit of Work implementation providing transaction management, automatic audit trail, and domain event dispatching.
/// Follows async-first design with sealed audit methods to prevent bypass vulnerabilities.
/// </summary>
/// <typeparam name="TDbContext">The DbContext type for database operations.</typeparam>
public abstract class UnitOfWork<TDbContext> : IUnitOfWork
    where TDbContext : DbContext
{
    protected readonly TDbContext Context;
    protected readonly IDomainEventDispatcher DomainEventDispatcher;
    protected string Username { get; init; }

    protected UnitOfWork(
        TDbContext dbContext,
        ICurrentUserService currentUserService,
        IDomainEventDispatcher domainEventDispatcher)
    {
        Context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        DomainEventDispatcher = domainEventDispatcher ?? throw new ArgumentNullException(nameof(domainEventDispatcher));
        Username = currentUserService?.Username ?? "system";
    }

    /// <summary>
    /// Commits changes to the database with automatic audit trail updates and domain event dispatching.
    /// Updates CreatedBy/CreatedDate for new entities and ModifiedBy/ModifiedDate for changed entities.
    /// Dispatches domain events after successful save to maintain consistency.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Number of entities written to the database.</returns>
    public async Task<int> CommitAsync(CancellationToken ct = default)
    {
        UpdateAuditableProperties();

        // Collect domain events from tracked aggregates BEFORE saving
        // Events are cleared from aggregates after collection
        var domainEvents = GetDomainEventsFromTrackedAggregates();

        // Save changes (transaction boundary)
        var result = await Context.SaveChangesAsync(ct);

        // Dispatch events AFTER successful save (eventual consistency)
        // Events are only published if SaveChanges succeeds
        await DomainEventDispatcher.DispatchAsync(domainEvents, ct);

        return result;
    }

    /// <summary>
    /// Asynchronously disposes the database context and releases resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
    }

    /// <summary>
    /// Updates audit properties for tracked entities. Not virtual to prevent audit bypass vulnerabilities.
    /// Derived classes cannot suppress or tamper with audit trail creation.
    /// </summary>
    protected void UpdateAuditableProperties()
    {
        var entries = Context.ChangeTracker
            .Entries<IEntity>()
            .Where(x => x.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.SetCreatedInfo(Username);
            }
            else
            {
                entry.Entity.SetUpdatedInfo(Username);
            }
        }
    }

    /// <summary>
    /// Collects domain events from all tracked aggregate roots and clears them from the aggregates.
    /// Called before SaveChanges to capture events for post-save dispatching.
    /// </summary>
    /// <returns>List of domain events to be dispatched after successful save.</returns>
    private List<DomainEvent> GetDomainEventsFromTrackedAggregates()
    {
        var aggregateRoots = Context.ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = aggregateRoots
            .SelectMany(aggregate => aggregate.DomainEvents)
            .ToList();

        // Clear events from aggregates after collecting
        // Prevents duplicate dispatching if CommitAsync is called multiple times
        foreach (var aggregate in aggregateRoots)
        {
            aggregate.ClearDomainEvents();
        }

        return domainEvents;
    }
}