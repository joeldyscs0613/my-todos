using Microsoft.EntityFrameworkCore;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Repositories;
using MyTodos.Services.IdentityService.Application.Permissions;
using MyTodos.Services.IdentityService.Application.Permissions.Contracts;
using MyTodos.Services.IdentityService.Application.Permissions.Queries;
using MyTodos.Services.IdentityService.Domain.PermissionAggregate;

namespace MyTodos.Services.IdentityService.Infrastructure.Persistence.Permissions.Repositories;

/// <summary>
/// Read-only repository for Permission aggregate queries.
/// </summary>
public sealed class PermissionReadRepository(IdentityServiceDbContext context, ICurrentUserService currentUserService)
    : ReadEfRepository<Permission, Guid, IdentityServiceDbContext>(context, new PermissionQueryConfiguration(), currentUserService),
        IPermissionReadRepository
{
    public async Task<Permission?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await GetFirstOrDefaultAsync(p => p.Code == code, ct);
}
