using Microsoft.EntityFrameworkCore;
using MyTodos.BuildingBlocks.Application.Contracts.Queries;
using MyTodos.Services.IdentityService.Domain.UserAggregate;

namespace MyTodos.Services.IdentityService.Application.Users.Queries;

public sealed class UserQueryConfiguration :  IEntityQueryConfiguration<User>
{
    public IQueryable<User> ConfigureAggregate(IQueryable<User> query)
    {
        return query
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission);
    }
}