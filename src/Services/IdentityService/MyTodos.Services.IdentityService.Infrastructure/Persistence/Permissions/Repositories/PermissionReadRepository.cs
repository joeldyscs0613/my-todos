using Microsoft.EntityFrameworkCore;
using MyTodos.Services.IdentityService.Application.Permissions.Contracts;
using MyTodos.Services.IdentityService.Domain.PermissionAggregate;
using MyTodos.Services.IdentityService.Infrastructure.Persistence;

namespace MyTodos.Services.IdentityService.Infrastructure.PermissionAggregate.Repositories;

/// <summary>
/// Read-only repository for Permission aggregate queries.
/// </summary>
public sealed class PermissionReadRepository : IPermissionReadRepository
{
    private readonly IdentityServiceDbContext _context;

    public PermissionReadRepository(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Permission?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Permissions
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Permission?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _context.Permissions
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Code == code, ct);
    }

    public async Task<IReadOnlyList<Permission>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Permissions
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Permission>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        return await _context.Permissions
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(ct);
    }
}
