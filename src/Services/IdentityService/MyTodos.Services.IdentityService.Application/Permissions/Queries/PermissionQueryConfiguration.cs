using Microsoft.EntityFrameworkCore;
using MyTodos.BuildingBlocks.Application.Contracts.Queries;
using MyTodos.Services.IdentityService.Domain.PermissionAggregate;

namespace MyTodos.Services.IdentityService.Application.Permissions.Queries;

public sealed class PermissionQueryConfiguration : IEntityQueryConfiguration<Permission>
{
    public IQueryable<Permission> ConfigureAggregate(IQueryable<Permission> query)
    {
        return query
            .Include(p => p.RolePermissions)
            .ThenInclude(ur => ur.Role);
    }
}