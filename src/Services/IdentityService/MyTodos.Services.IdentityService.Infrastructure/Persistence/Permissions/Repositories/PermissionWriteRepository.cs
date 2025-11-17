using MyTodos.Services.IdentityService.Application.Permissions.Contracts;
using MyTodos.Services.IdentityService.Domain.PermissionAggregate;
using MyTodos.Services.IdentityService.Infrastructure.Persistence;

namespace MyTodos.Services.IdentityService.Infrastructure.PermissionAggregate.Repositories;

/// <summary>
/// Write repository for Permission aggregate mutations.
/// </summary>
public sealed class PermissionWriteRepository : IPermissionWriteRepository
{
    private readonly IdentityServiceDbContext _context;

    public PermissionWriteRepository(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Permission permission, CancellationToken ct = default)
    {
        await _context.Permissions.AddAsync(permission, ct);
    }

    public Task UpdateAsync(Permission permission, CancellationToken ct = default)
    {
        _context.Permissions.Update(permission);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Permission permission, CancellationToken ct = default)
    {
        _context.Permissions.Remove(permission);
        return Task.CompletedTask;
    }
}
