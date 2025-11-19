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
    private readonly ICurrentUserService _currentUserService;
    protected string Username { get; init; }

    protected UnitOfWork(
        TDbContext dbContext,
        ICurrentUserService currentUserService,
        IDomainEventDispatcher domainEventDispatcher)
    {
        Context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        DomainEventDispatcher = domainEventDispatcher ?? throw new ArgumentNullException(nameof(domainEventDispatcher));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        Username = currentUserService.Username ?? "system";
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
        UpdateTenantProperties();

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
    /// Updates and validates tenant properties for multi-tenant entities. Not virtual to prevent tenant isolation bypass.
    /// Ensures TenantId is set correctly for new entities and validates tenant ownership for modifications
    /// and set it correctly no matter where in the app had been updated by mistake.
    /// </summary>
    private void UpdateTenantProperties()
    {
        if (!_currentUserService.TenantId.HasValue)
        {
            // If there are any multi-tenant entities being tracked, this is an error
            var hasMultiTenantEntities = Context.ChangeTracker.Entries()
                .Any(e => e.Entity is IMultiTenantEntity &&
                         (e.State == EntityState.Added || e.State == EntityState.Modified));

            if (hasMultiTenantEntities)
            {
                throw new InvalidOperationException(
                    "TenantId is not available from CurrentUserService, but multi-tenant entities are being persisted.");
            }

            return; // No tenant context, nothing to do
        }

        var currentTenantId = _currentUserService.TenantId.Value;
        var entries = Context.ChangeTracker.Entries()
            .Where(e => e.Entity is IMultiTenantEntity &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var tenantEntity = (IMultiTenantEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                // For new entities, always set to current user's tenant
                tenantEntity.SetTenantId(currentTenantId);
            }
            else if (entry.State == EntityState.Modified)
            {
                // For modified entities, validate and auto-correct
                var originalTenantId = (Guid)entry.OriginalValues[nameof(IMultiTenantEntity.TenantId)];

                // First check: Does the original entity belong to the current user?
                if (originalTenantId != currentTenantId)
                {
                    throw new UnauthorizedAccessException(
                        $"Cannot modify entity belonging to TenantId '{originalTenantId}' " +
                        $"- current user belongs to TenantId '{currentTenantId}'");
                }

                // Second check: Was TenantId tampered with? Auto-correct it
                if (tenantEntity.TenantId != originalTenantId)
                {
                    // Reset to original value (auto-correct potential tampering)
                    tenantEntity.SetTenantId(originalTenantId);
                }
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