using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.Services.IdentityService.Domain.TenantAggregate.DomainEvents;

/// <summary>
/// Domain event raised when a new tenant is created
/// </summary>
public sealed record TenantCreatedDomainEvent : DomainEvent
{
    public Guid TenantId { get; init; }
    public string Name { get; init; }

    public TenantCreatedDomainEvent(Guid tenantId, string name)
        : base("TenantCreated", "Tenant", tenantId.ToString())
    {
        TenantId = tenantId;
        Name = name;
    }

    // For deserialization
    [Obsolete("Only for deserialization. Use parameterized constructor.", error: true)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    private TenantCreatedDomainEvent() : base() { }
}
