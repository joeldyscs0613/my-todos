using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Repositories;
using MyTodos.Services.IdentityService.Application.Roles.Contracts;
using MyTodos.Services.IdentityService.Application.Roles.Queries.GetPagedList;
using MyTodos.Services.IdentityService.Domain.RoleAggregate;
using MyTodos.Services.IdentityService.Infrastructure.Persistence;

namespace MyTodos.Services.IdentityService.Infrastructure.Persistence.Roles.Repositories;

/// <summary>
/// Repository for paged list queries on roles.
/// </summary>
public sealed class RolePagedListReadRepository(IdentityServiceDbContext context, ICurrentUserService currentUserService)
    : PagedListReadEfRepository<Role, Guid, RolePagedListSpecification, RolePagedListFilter,
            IdentityServiceDbContext>(context, new RoleQueryConfiguration(), currentUserService)
        , IRolePagedListReadRepository
{
}
