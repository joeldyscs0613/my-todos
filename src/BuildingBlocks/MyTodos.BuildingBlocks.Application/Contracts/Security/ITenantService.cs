namespace MyTodos.BuildingBlocks.Application.Contracts.Security;

/// <summary>
/// Service for tenant resolution and validation in multi-tenant applications.
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Get the current tenant ID from the request context (JWT claim or header).
    /// </summary>
    /// <returns>The current tenant ID, or null if not available</returns>
    Guid? GetCurrentTenantId();

    /// <summary>
    /// Validate if the current user has access to the specified tenant.
    /// </summary>
    /// <param name="tenantId">The tenant ID to validate access for</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if the user has access to the tenant, false otherwise</returns>
    Task<bool> ValidateTenantAccessAsync(Guid tenantId, CancellationToken ct = default);
}
