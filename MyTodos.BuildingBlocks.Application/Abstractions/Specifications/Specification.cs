using System.Linq.Expressions;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MyTodos.BuildingBlocks.Application.Abstractions.Filters;
using MyTodos.BuildingBlocks.Application.Constants;
using MyTodos.BuildingBlocks.Application.Contracts;
using MyTodos.SharedKernel.Abstractions;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.BuildingBlocks.Application.Abstractions.Specifications;

public abstract class Specification<TEntity, TId, TFilter> : ISpecification<TEntity, TFilter>
    where TFilter : Filter
    where TEntity : Entity<TId>
    where TId : IComparable
{
    public int TotalCount { get; protected set; }
    
    public TFilter Filter { get; }

    public int PageNumber => Filter.PageNumber;
    public int PageSize => Filter.PageSize;

    public string? SortField => Filter.SortField;
    public string? SortDirection => Filter.SortDirection;
    
    protected Specification(TFilter filter)
    {
        Filter = filter;
    }
    
    public virtual Result<IQueryable<TEntity>> Apply(IQueryable<TEntity> query)
    {
        query = ApplyIncludes(query);

        query = ApplyFilter(query);

        if (!string.IsNullOrWhiteSpace(Filter.SearchBy))
        {
            query = ApplySearchBy(query);
        }

        var sortResult = ApplySort(query);
        if (sortResult.IsFailure)
        {
            return sortResult;
        }
        query = sortResult.Value;

        query = ApplyPaging(query);

        return Result.Success(query);
    }
    
    public virtual async Task CountAsync(IQueryable<TEntity> query, CancellationToken ct = default)
    {
        query = ApplyIncludes(query);
        
        query = ApplyFilter(query);
        
        if (!string.IsNullOrWhiteSpace(Filter.SearchBy))
        {
            query = ApplySearchBy(query);
        }
        
        TotalCount = await query.CountAsync(ct);
    }

    protected abstract IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> query);

    protected abstract IQueryable<TEntity> ApplyFilter(IQueryable<TEntity> query);

    protected abstract IQueryable<TEntity> ApplySearchBy(IQueryable<TEntity> query);

    protected abstract Dictionary<string, Expression<Func<TEntity, object>>> GetSortFunctions();
    
    protected Result<IQueryable<TEntity>> ApplySort(IQueryable<TEntity> query)
    {
        if (string.IsNullOrWhiteSpace(SortField))
        {
            return Result.Success(query);
        }

        var sortField = SortField;
        var sortFunctions = GetSortFunctions();
        if (sortFunctions.TryGetValue(sortField, out var func1))
        {
            query = string.Equals("desc", Filter.SortDirection, StringComparison.OrdinalIgnoreCase)
                ? query.OrderByDescending(func1)
                : query.OrderBy(func1);
        }
        else
        {
            var sb = new StringBuilder();
            sb.Append(SortField.Substring(0, 1).ToUpper());
            sb.Append(SortField.Substring(1));
            sortField = sb.ToString();

            if (!sortFunctions.TryGetValue(sortField, out var func2))
            {
                return Result.BadRequest<IQueryable<TEntity>>($"{SortField} is not a valid sort field name.");
            }

            query = string.Equals("desc", Filter.SortDirection, StringComparison.OrdinalIgnoreCase)
                ? query.OrderByDescending(func2)
                : query.OrderBy(func2);
        }

        return Result.Success(query);
    }
    
    protected IQueryable<TEntity> ApplyPaging(IQueryable<TEntity> query)
    {
        var pageNumber = Filter.PageNumber;

        var pageSize = PageSize > PageListConstants.MaxPageSize
            ? PageListConstants.MaxPageSize
            : PageSize < PageListConstants.MinPageSize
                ? PageListConstants.MinPageSize
                : Filter.PageSize;

        if (pageSize <= 0)
        {
            pageSize = PageListConstants.DefaultPageSize;
        }
            
        Filter.PageSize = pageSize;
        query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        
        return query;
    }
}