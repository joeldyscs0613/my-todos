namespace MyTodos.BuildingBlocks.Application.Contracts.Queries;

/// <summary>
/// Configures queries to load complete aggregates with consistent includes and filters.
/// Implement once per aggregate root and inject into repositories for DDD aggregate boundary enforcement.
/// </summary>
/// <typeparam name="TEntity">The entity type to configure queries for.</typeparam>
/// <remarks>
/// Repositories automatically apply this configuration for entity operations. Specifications
/// use it by default but can override for lightweight DTOs.
/// </remarks>
public interface IEntityQueryConfiguration<TEntity> where TEntity : class
{
    /// <summary>
    /// Configures a query to load a complete aggregate with includes, joins, and filters.
    /// </summary>
    /// <param name="query">The base query to configure.</param>
    /// <returns>The configured query ready for execution.</returns>
    IQueryable<TEntity> ConfigureAggregate(IQueryable<TEntity> query);
}
