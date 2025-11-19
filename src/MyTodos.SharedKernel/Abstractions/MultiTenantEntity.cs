using MyTodos.SharedKernel.Contracts;

namespace MyTodos.SharedKernel.Abstractions;

/// <summary>
/// Base class for multi-tenant entities (non-aggregate roots) in the domain model.
/// Extends Entity with tenant isolation support via TenantId.
/// Use this base class for child entities within aggregates that require multi-tenant data isolation.
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>
public abstract class MultiTenantEntity<TId> : Entity<TId>, IMultiTenantEntity
    where TId : IComparable
{
    /// <summary>
    /// Gets or sets the tenant identifier for multi-tenant data isolation.
    /// This property enforces that data belongs to a specific tenant and cannot be accessed by other tenants.
    /// The setter is provided for EF Core, but should not be used in domain code.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Parameterless constructor for deserialization only (EF Core, JSON serializers).
    /// DO NOT USE in domain code.
    /// </summary>
    [Obsolete("Only for deserialization. Use parameterized constructor.", error: true)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    protected MultiTenantEntity() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the multi-tenant entity with the specified identifier and tenant.
    /// </summary>
    /// <param name="id">The unique identifier for the entity.</param>
    /// <param name="tenantId">The tenant identifier for data isolation.</param>
    protected MultiTenantEntity(TId id, Guid tenantId) : base(id)
    {
        TenantId = tenantId;
    }

    /// <summary>
    /// Sets the tenant identifier for this entity.
    /// Called by the UnitOfWork during entity persistence to ensure tenant data isolation.
    /// </summary>
    /// <param name="tenantId">The tenant identifier to assign to this entity.</param>
    public void SetTenantId(Guid tenantId)
    {
        TenantId = tenantId;
    }
}
