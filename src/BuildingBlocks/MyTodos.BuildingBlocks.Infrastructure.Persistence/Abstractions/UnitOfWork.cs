using Microsoft.EntityFrameworkCore;
using MyTodos.BuildingBlocks.Application.Contracts;
using MyTodos.SharedKernel.Contracts;

namespace MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions;

/// <summary>
/// Base Unit of Work implementation providing transaction management and automatic audit trail.
/// Follows async-first design with sealed audit methods to prevent bypass vulnerabilities.
/// </summary>
/// <typeparam name="TDbContext">The DbContext type for database operations.</typeparam>
public abstract class UnitOfWork<TDbContext> : IUnitOfWork
    where TDbContext : DbContext
{
    protected readonly TDbContext Context;
    protected string Username { get; init; }
    
    protected UnitOfWork(
        TDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        Context = dbContext;
        Username = currentUserService.Username ?? "system";
    }

    /// <summary>
    /// Commits changes to the database with automatic audit trail updates.
    /// Updates CreatedBy/CreatedDate for new entities and ModifiedBy/ModifiedDate for changed entities.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Number of entities written to the database.</returns>
    public async Task<int> CommitAsync(CancellationToken ct = default)
    {
        UpdateAuditableProperties();

        return await Context.SaveChangesAsync(ct);
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
}