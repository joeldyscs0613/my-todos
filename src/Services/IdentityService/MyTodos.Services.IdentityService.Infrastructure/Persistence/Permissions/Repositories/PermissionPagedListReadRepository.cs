using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Repositories;
using MyTodos.Services.IdentityService.Application.Permissions.Contracts;
using MyTodos.Services.IdentityService.Application.Permissions.Queries.GetPagedList;
using MyTodos.Services.IdentityService.Domain.PermissionAggregate;
using MyTodos.Services.IdentityService.Infrastructure.Persistence;

namespace MyTodos.Services.IdentityService.Infrastructure.Persistence.Permissions.Repositories;

/// <summary>
/// Repository for paged list queries on permissions.
/// </summary>
public sealed class PermissionPagedListReadRepository(IdentityServiceDbContext context, ICurrentUserService currentUserService)
    : PagedListReadEfRepository<Permission, Guid, PermissionPagedListSpecification, PermissionPagedListFilter,
            IdentityServiceDbContext>(context, new PermissionQueryConfiguration(), currentUserService)
        , IPermissionPagedListReadRepository
{
}
