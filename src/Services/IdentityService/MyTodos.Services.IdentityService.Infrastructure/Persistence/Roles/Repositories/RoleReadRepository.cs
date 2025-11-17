using Microsoft.EntityFrameworkCore;
using MyTodos.Services.IdentityService.Application.Roles.Contracts;
using MyTodos.Services.IdentityService.Domain.RoleAggregate;
using MyTodos.Services.IdentityService.Infrastructure.Persistence;

namespace MyTodos.Services.IdentityService.Infrastructure.RoleAggregate.Repositories;

/// <summary>
/// Read-only repository for Role aggregate queries.
/// </summary>
public sealed class RoleReadRepository : IRoleReadRepository
{
    private readonly IdentityServiceDbContext _context;

    public RoleReadRepository(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<Role?> GetByIdWithPermissionsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Roles
            .AsNoTracking()
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<Role?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Code == code, ct);
    }

    public async Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Roles
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Role>> GetByScopeAsync(Domain.RoleAggregate.Enums.AccessScope scope, CancellationToken ct = default)
    {
        return await _context.Roles
            .AsNoTracking()
            .Where(r => r.Scope == scope)
            .ToListAsync(ct);
    }
}
