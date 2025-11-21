using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Repositories;
using MyTodos.Services.IdentityService.Application.Roles.Contracts;
using MyTodos.Services.IdentityService.Application.Roles.Queries;
using MyTodos.Services.IdentityService.Domain.RoleAggregate;

namespace MyTodos.Services.IdentityService.Infrastructure.Persistence.Roles.Repositories;

/// <summary>
/// Read-only repository for Role aggregate queries.
/// </summary>
public sealed class RoleReadRepository(IdentityServiceDbContext context, ICurrentUserService currentUserService)
    : ReadEfRepository<Role, Guid, IdentityServiceDbContext>(context, new RoleQueryConfiguration(), currentUserService),
        IRoleReadRepository
{
    public async Task<Role?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await GetFirstOrDefaultAsync(p => p.Code == code, ct);

    public async Task<IReadOnlyList<Role>> GetAllByScopeAsync(
        Domain.RoleAggregate.Enums.AccessScope scope, CancellationToken ct = default)
        => await GetAllAsync(r => r.Scope == scope, ct);
}
