using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.Services.IdentityService.Domain.TenantAggregate;

namespace MyTodos.Services.IdentityService.Application.Tenants.Contracts;

/// <summary>
/// Read repository for Tenant aggregate.
/// </summary>
public interface ITenantReadRepository : IReadRepository<Tenant, Guid>
{
    /// <summary>
    /// Get tenant by name
    /// </summary>
    Task<Tenant?> GetByNameAsync(string name, CancellationToken ct = default);
}
