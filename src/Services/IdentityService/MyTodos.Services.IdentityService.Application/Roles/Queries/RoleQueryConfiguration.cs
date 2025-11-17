using MyTodos.BuildingBlocks.Application.Contracts.Queries;
using MyTodos.Services.IdentityService.Domain.RoleAggregate;

namespace MyTodos.Services.IdentityService.Application.Roles;

public sealed class RoleQueryConfiguration : IEntityQueryConfiguration<Role>
{
    public IQueryable<Role> ConfigureAggregate(IQueryable<Role> query)
    {
        return query;
    }
}