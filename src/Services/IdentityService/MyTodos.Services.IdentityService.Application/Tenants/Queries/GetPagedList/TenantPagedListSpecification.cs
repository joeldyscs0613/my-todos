using System.Linq.Expressions;
using MyTodos.BuildingBlocks.Application.Abstractions.Specifications;
using MyTodos.Services.IdentityService.Domain.TenantAggregate;

namespace MyTodos.Services.IdentityService.Application.Tenants.Queries.GetPagedList;

public sealed class TenantPagedListSpecification(TenantPagedListFilter filter)
    : Specification<Tenant, Guid, TenantPagedListFilter>(filter)
{
    protected override IQueryable<Tenant> ApplyFilter(IQueryable<Tenant> query)
    {
        if (!string.IsNullOrWhiteSpace(Filter.Name))
        {
            query = query.Where(t => t.Name.Contains(Filter.Name));
        }
        
        return query;
    }

    protected override IQueryable<Tenant> ApplySearchBy(IQueryable<Tenant> query)
    {
        if (!string.IsNullOrWhiteSpace(Filter.SearchBy))
        {
            query = query.Where(t => t.Name.Contains(Filter.SearchBy));
        }
        
        return query;
    }

    protected override Dictionary<string, Expression<Func<Tenant, object>>> GetSortFunctions()
    {
        
    }
}