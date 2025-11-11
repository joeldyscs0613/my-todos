using System.Linq.Expressions;
using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.BuildingBlocks.Application.Contracts;

public interface IReadRepository<TEntity, in TId>
    where TEntity : AggregateRoot<TId>
    where TId : IComparable
{
    Task<IReadOnlyList<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? predicate, CancellationToken ct);
    
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken ct);
    
    Task<TEntity?> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct);
    
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> expression, CancellationToken ct);
}