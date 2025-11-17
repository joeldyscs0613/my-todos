namespace MyTodos.Services.IdentityService.Domain.TenantAggregate.Contracts;

/// <summary>
/// Write repository for Tenant aggregate.
/// </summary>
public interface ITenantWriteRepository
{
    /// <summary>
    /// Add a new tenant
    /// </summary>
    Task AddAsync(Tenant tenant, CancellationToken ct = default);

    /// <summary>
    /// Update an existing tenant
    /// </summary>
    Task UpdateAsync(Tenant tenant, CancellationToken ct = default);

    /// <summary>
    /// Delete a tenant
    /// </summary>
    Task DeleteAsync(Tenant tenant, CancellationToken ct = default);
}
