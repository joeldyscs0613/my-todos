using FluentValidation;

namespace MyTodos.BuildingBlocks.Application.Validators;

/// <summary>
/// Base validator class for queries.
/// Provides a common base for all query validators without making assumptions about query structure.
/// </summary>
/// <typeparam name="TQuery">The type of query being validated.</typeparam>
/// <remarks>
/// This is a minimal base class that can be used for any query type.
/// For paged queries with sorting and filtering, use <see cref="PagedListQueryValidator{TQuery, TSpecification, TFilter, TResponseItemDto}"/> instead.
/// </remarks>
public abstract class QueryValidator<TQuery> : AbstractValidator<TQuery>
    where TQuery : class
{
}
