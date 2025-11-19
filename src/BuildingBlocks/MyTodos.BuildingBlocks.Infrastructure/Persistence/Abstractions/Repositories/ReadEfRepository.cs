using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MyTodos.BuildingBlocks.Application.Contracts;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Queries;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.SharedKernel.Abstractions;
using MyTodos.SharedKernel.Contracts;

namespace MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Repositories;

/// <summary>
/// Base class for EF Core read-only repositories with centralized query configuration.
/// Provides no-tracking queries and consistent aggregate loading via IEntityQueryConfiguration.
/// </summary>
public abstract class ReadEfRepository<TEntity, TId, TDbContext> : IReadRepository<TEntity, TId>
    where TEntity : AggregateRoot<TId>
    where TId : IComparable
    where TDbContext : DbContext
{
    protected readonly TDbContext Context;
    protected readonly IEntityQueryConfiguration<TEntity> QueryConfiguration;
    protected readonly ICurrentUserService _currentUserService;

    protected ReadEfRepository(TDbContext context, IEntityQueryConfiguration<TEntity> queryConfiguration, ICurrentUserService currentUserService)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        QueryConfiguration = queryConfiguration ?? throw new ArgumentNullException(nameof(queryConfiguration));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    /// <summary>
    /// Retrieves all entities matching the optional predicate.
    /// Uses centralized aggregate configuration to load complete entities.
    /// </summary>
    /// <param name="predicate">Optional filter condition. If null, returns all entities.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only list of all entities matching the predicate.</returns>
    /// <remarks>
    /// This method returns complete entities with all configured includes from IEntityQueryConfiguration.
    /// For dropdown/option data, use GetAsOptionsAsync instead for better performance.
    /// For paginated results, use IPagedListReadRepository.GetPagedListAsync.
    /// </remarks>
    public async Task<IReadOnlyList<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? predicate, CancellationToken ct)
    {
        var query = GetInitialQueryForEntity();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        var entities = await query.ToListAsync(ct);

        return entities;
    }

    /// <summary>
    /// Retrieves entities optimized for dropdown/select list options. No includes applied for performance.
    /// Override to add filtering (e.g., active only) or custom sort order.
    /// </summary>
    public virtual async Task<IReadOnlyList<TEntity>> GetAsOptionsAsync(CancellationToken ct)
    {
        var query = Context.Set<TEntity>().AsNoTracking();

        var entities = await query.ToListAsync(ct);

        return entities;
    }

    /// <summary>
    /// Retrieves a single entity by its unique identifier.
    /// Returns the fully-loaded aggregate with all related entities using centralized configuration.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The entity if found; otherwise null.</returns>
    public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken ct)
    {
        var entity = await GetInitialQueryForEntity().FirstOrDefaultAsync(e => e.Id.Equals(id), ct);
        return ValidateTenantAccess(entity);
    }

    /// <summary>
    /// Retrieves the first entity matching the predicate.
    /// Returns the fully-loaded aggregate with all related entities using centralized configuration.
    /// </summary>
    /// <param name="predicate">Filter condition to match entities.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The first matching entity if found; otherwise null.</returns>
    public virtual async Task<TEntity?> GetFirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate, CancellationToken ct)
        => await GetInitialQueryForEntity().FirstOrDefaultAsync(predicate, ct);

    /// <summary>
    /// Checks whether any entity exists that matches the predicate.
    /// More efficient than fetching the entity when you only need to verify existence.
    /// </summary>
    /// <param name="predicate">Filter condition to test.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if at least one matching entity exists; otherwise false.</returns>
    public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct)
        => await GetInitialQueryForEntity().AnyAsync(predicate, ct);

    /// <summary>
    /// Builds the base query for entity retrieval operations with aggregate configuration applied.
    /// Used by GetByIdAsync, GetFirstOrDefaultAsync, GetAllAsync, and ExistsAsync.
    /// </summary>
    protected virtual IQueryable<TEntity> GetInitialQueryForEntity()
    {
        var query = Context.Set<TEntity>().AsNoTracking();
        query = QueryConfiguration.ConfigureAggregate(query);
        return query;
    }

    /// <summary>
    /// Builds a clean base query for specification-based list operations.
    /// Configuration is passed to specifications, which decide whether to use or override it.
    /// </summary>
    protected virtual IQueryable<TEntity> GetInitialQueryForList()
    {
        return Context.Set<TEntity>().AsNoTracking();
    }

    /// <summary>
    /// Validates that the entity belongs to the current user's tenant.
    /// Returns null if the entity doesn't belong to the current tenant (returns 404 to caller).
    /// Override this method to customize tenant validation behavior.
    /// </summary>
    /// <param name="entity">The entity to validate.</param>
    /// <returns>The entity if valid, null if cross-tenant access detected.</returns>
    protected virtual TEntity? ValidateTenantAccess(TEntity? entity)
    {
        if (entity != null && entity is IMultiTenantEntity tenantEntity && _currentUserService.TenantId.HasValue)
        {
            if (tenantEntity.TenantId != _currentUserService.TenantId.Value)
            {
                return null; // Cross-tenant access - return null (caller will return 404)
            }
        }
        return entity;
    }
}