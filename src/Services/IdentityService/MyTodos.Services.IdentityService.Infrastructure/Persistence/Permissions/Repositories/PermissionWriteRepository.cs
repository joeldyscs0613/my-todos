using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Repositories;
using MyTodos.Services.IdentityService.Application.Permissions;
using MyTodos.Services.IdentityService.Application.Permissions.Contracts;
using MyTodos.Services.IdentityService.Application.Permissions.Queries;
using MyTodos.Services.IdentityService.Domain.PermissionAggregate;

namespace MyTodos.Services.IdentityService.Infrastructure.Persistence.Permissions.Repositories;

/// <summary>
/// Write repository for Permission aggregate mutations.
/// </summary>
public sealed class PermissionWriteRepository(IdentityServiceDbContext context)
    : WriteEfRepository<Permission, Guid, IdentityServiceDbContext>(context, new PermissionQueryConfiguration()),
        IPermissionWriteRepository;
