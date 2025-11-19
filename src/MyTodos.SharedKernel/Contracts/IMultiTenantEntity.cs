namespace MyTodos.SharedKernel.Contracts;

/// <summary>
/// Marker interface for entities that belong to a specific tenant.
/// Entities implementing this interface will have tenant-scoped query filters applied automatically.
/// </summary>
/// <remarks>
/// Use this interface for entities that should be isolated per tenant in a multi-tenant application.
/// Entities that are global (like Tenant itself, Roles, or system configuration) should NOT implement this interface.
/// </remarks>
public interface IMultiTenantEntity
{
    /// <summary>
    /// The unique identifier of the tenant this entity belongs to.
    /// </summary>
    Guid TenantId { get; set; }

    /// <summary>
    /// Sets the tenant identifier for this entity.
    /// Called by the UnitOfWork during entity persistence to ensure tenant data isolation.
    /// </summary>
    /// <param name="tenantId">The tenant identifier to assign to this entity.</param>
    void SetTenantId(Guid tenantId);
}
