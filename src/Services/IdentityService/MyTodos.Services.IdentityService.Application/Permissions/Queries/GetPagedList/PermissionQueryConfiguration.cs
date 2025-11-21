using Microsoft.EntityFrameworkCore;
using MyTodos.BuildingBlocks.Application.Contracts.Queries;
using MyTodos.Services.IdentityService.Domain.PermissionAggregate;

namespace MyTodos.Services.IdentityService.Application.Permissions.Queries.GetPagedList;

/// <summary>
/// Query configuration for permission aggregates.
/// </summary>
public sealed class PermissionQueryConfiguration : IEntityQueryConfiguration<Permission>
{
    public IQueryable<Permission> ConfigureAggregate(IQueryable<Permission> query)
    {
        // Simplified to avoid circular includes
        return query
            .Include(p => p.RolePermissions)
            .ThenInclude(p => p.Role);
    }
}