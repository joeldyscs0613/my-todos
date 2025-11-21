using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.Services.IdentityService.Application.Roles.Queries.GetPagedList;
using MyTodos.Services.IdentityService.Domain.RoleAggregate;

namespace MyTodos.Services.IdentityService.Application.Roles.Contracts;

/// <summary>
/// Repository for paged list queries on roles.
/// </summary>
public interface IRolePagedListReadRepository
    : IPagedListReadRepository<Role, Guid, RolePagedListSpecification, RolePagedListFilter>
{
}
