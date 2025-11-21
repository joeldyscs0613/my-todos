using System.Linq.Expressions;
using MyTodos.BuildingBlocks.Application.Abstractions.Specifications;
using MyTodos.Services.IdentityService.Domain.RoleAggregate;

namespace MyTodos.Services.IdentityService.Application.Roles.Queries.GetPagedList;

/// <summary>
/// Specification for filtering and sorting roles.
/// </summary>
public sealed class RolePagedListSpecification(RolePagedListFilter filter)
    : Specification<Role, Guid, RolePagedListFilter>(filter)
{
    protected override IQueryable<Role> ApplyFilter(IQueryable<Role> query)
    {
        if (!string.IsNullOrWhiteSpace(Filter.Code))
        {
            query = query.Where(r => r.Code.Contains(Filter.Code));
        }

        if (!string.IsNullOrWhiteSpace(Filter.Name))
        {
            query = query.Where(r => r.Name.Contains(Filter.Name));
        }

        if (Filter.Scope.HasValue)
        {
            query = query.Where(r => r.Scope == Filter.Scope.Value);
        }

        return query;
    }

    protected override IQueryable<Role> ApplySearchBy(IQueryable<Role> query)
    {
        if (!string.IsNullOrWhiteSpace(Filter.SearchBy))
        {
            query = query.Where(r => r.Code.Contains(Filter.SearchBy) || r.Name.Contains(Filter.SearchBy));
        }

        return query;
    }

    protected override Dictionary<string, Expression<Func<Role, object>>> GetSortFunctions()
    {
        return new Dictionary<string, Expression<Func<Role, object>>>
        {
            { nameof(Filter.Code), r => r.Code },
            { nameof(Filter.Name), r => r.Name },
            { nameof(Filter.Scope), r => r.Scope }
        };
    }
}
