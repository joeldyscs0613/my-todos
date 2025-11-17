using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Repositories;
using MyTodos.Services.IdentityService.Application.Roles;
using MyTodos.Services.IdentityService.Application.Roles.Contracts;
using MyTodos.Services.IdentityService.Application.Roles.Queries;
using MyTodos.Services.IdentityService.Domain.RoleAggregate;
using MyTodos.Services.IdentityService.Infrastructure.Persistence;

namespace MyTodos.Services.IdentityService.Infrastructure.RoleAggregate.Repositories;

/// <summary>
/// Read-only repository for Role aggregate queries.
/// </summary>
public sealed class RoleReadRepository(IdentityServiceDbContext context)
    : ReadEfRepository<Role, Guid, IdentityServiceDbContext>(context, new RoleQueryConfiguration()),
        IRoleReadRepository
{
    public async Task<Role?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await GetFirstOrDefaultAsync(p => p.Code == code, ct);
    
    public async Task<IReadOnlyList<Role>> GetAllByScopeAsync(
        Domain.RoleAggregate.Enums.AccessScope scope, CancellationToken ct = default)
        => await GetAllAsync(r => r.Scope == scope, ct);
}
