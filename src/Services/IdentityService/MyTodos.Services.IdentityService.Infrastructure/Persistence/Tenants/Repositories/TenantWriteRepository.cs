using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.IdentityService.Application.Tenants.Contracts;
using MyTodos.Services.IdentityService.Domain.TenantAggregate;

namespace MyTodos.Services.IdentityService.Infrastructure.Persistence.Tenants.Repositories;

/// <summary>
/// Write repository for Tenant aggregate mutations.
/// </summary>
public sealed class TenantWriteRepository : ITenantWriteRepository
{
    private readonly IdentityServiceDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public TenantWriteRepository(IdentityServiceDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task AddAsync(Tenant tenant, CancellationToken ct = default)
    {
        await _context.Tenants.AddAsync(tenant, ct);
    }

    public Task UpdateAsync(Tenant tenant, CancellationToken ct = default)
    {
        _context.Tenants.Update(tenant);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Tenant tenant, CancellationToken ct = default)
    {
        _context.Tenants.Remove(tenant);
        return Task.CompletedTask;
    }
}
