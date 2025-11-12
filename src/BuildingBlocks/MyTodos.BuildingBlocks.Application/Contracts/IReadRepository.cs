using System.Linq.Expressions;
using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.BuildingBlocks.Application.Contracts;

public interface IReadRepository<TEntity, in TId>
    where TEntity : AggregateRoot<TId>
    where TId : IComparable
{
    /// <summary>
    /// Retrieves a list of entities matching the optional predicate.
    /// </summary>
    Task<IReadOnlyList<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken ct);

    /// <summary>
    /// Retrieves a single entity by its identifier.
    /// </summary>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken ct);

    /// <summary>
    /// Retrieves the first entity matching the predicate, or null if none found.
    /// </summary>
    Task<TEntity?> GetFirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct);

    /// <summary>
    /// Checks whether any entity exists matching the predicate.
    /// </summary>
    Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate, 
        CancellationToken ct);
}