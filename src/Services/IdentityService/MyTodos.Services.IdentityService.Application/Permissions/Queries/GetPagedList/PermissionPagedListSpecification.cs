using System.Linq.Expressions;
using MyTodos.BuildingBlocks.Application.Abstractions.Specifications;
using MyTodos.Services.IdentityService.Domain.PermissionAggregate;

namespace MyTodos.Services.IdentityService.Application.Permissions.Queries.GetPagedList;

/// <summary>
/// Specification for filtering and sorting permissions.
/// </summary>
public sealed class PermissionPagedListSpecification(PermissionPagedListFilter filter)
    : Specification<Permission, Guid, PermissionPagedListFilter>(filter)
{
    protected override IQueryable<Permission> ApplyFilter(IQueryable<Permission> query)
    {
        if (!string.IsNullOrWhiteSpace(Filter.Code))
        {
            query = query.Where(p => p.Code.Contains(Filter.Code));
        }

        if (!string.IsNullOrWhiteSpace(Filter.Name))
        {
            query = query.Where(p => p.Name.Contains(Filter.Name));
        }

        return query;
    }

    protected override IQueryable<Permission> ApplySearchBy(IQueryable<Permission> query)
    {
        if (!string.IsNullOrWhiteSpace(Filter.SearchBy))
        {
            query = query.Where(p => p.Code.Contains(Filter.SearchBy) || p.Name.Contains(Filter.SearchBy));
        }

        return query;
    }

    protected override Dictionary<string, Expression<Func<Permission, object>>> GetSortFunctions()
    {
        return new Dictionary<string, Expression<Func<Permission, object>>>
        {
            { nameof(Filter.Code), p => p.Code },
            { nameof(Filter.Name), p => p.Name }
        };
    }
}