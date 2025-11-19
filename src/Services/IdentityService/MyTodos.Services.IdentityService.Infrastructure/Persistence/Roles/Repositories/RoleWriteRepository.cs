using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.IdentityService.Application.Roles.Contracts;
using MyTodos.Services.IdentityService.Domain.RoleAggregate;
using MyTodos.Services.IdentityService.Infrastructure.Persistence;

namespace MyTodos.Services.IdentityService.Infrastructure.RoleAggregate.Repositories;

/// <summary>
/// Write repository for Role aggregate mutations.
/// </summary>
public sealed class RoleWriteRepository : IRoleWriteRepository
{
    private readonly IdentityServiceDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RoleWriteRepository(IdentityServiceDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task AddAsync(Role role, CancellationToken ct = default)
    {
        await _context.Roles.AddAsync(role, ct);
    }

    public Task UpdateAsync(Role role, CancellationToken ct = default)
    {
        _context.Roles.Update(role);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Role role, CancellationToken ct = default)
    {
        _context.Roles.Remove(role);
        return Task.CompletedTask;
    }
}
