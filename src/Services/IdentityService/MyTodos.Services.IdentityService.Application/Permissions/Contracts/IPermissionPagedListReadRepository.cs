using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.Services.IdentityService.Application.Permissions.Queries.GetPagedList;
using MyTodos.Services.IdentityService.Domain.PermissionAggregate;

namespace MyTodos.Services.IdentityService.Application.Permissions.Contracts;

/// <summary>
/// Repository for paged list queries on permissions.
/// </summary>
public interface IPermissionPagedListReadRepository
    : IPagedListReadRepository<Permission, Guid, PermissionPagedListSpecification, PermissionPagedListFilter>
{
}
