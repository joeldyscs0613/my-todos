using Microsoft.EntityFrameworkCore;
using MyTodos.BuildingBlocks.Application.Contracts.Queries;
using MyTodos.Services.IdentityService.Domain.TenantAggregate;

namespace MyTodos.Services.IdentityService.Application.Tenants.Queries.GetPagedList;

public sealed class TenantQueryConfiguration :  IEntityQueryConfiguration<Tenant>
{
    public IQueryable<Tenant> ConfigureAggregate(IQueryable<Tenant> query)
    {
        return query
            .Include(t => t.UserRoles)
            .ThenInclude(ur => ur.User)
            .ThenInclude(t => t.UserRoles)
            .ThenInclude(ur => ur.Role);
    }
}