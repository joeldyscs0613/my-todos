using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.Services.IdentityService.Domain.UserAggregate.DomainEvents;

/// <summary>
/// Domain event raised when a tenant-scoped role is assigned to a user
/// </summary>
public sealed record TenantRoleAssignedDomainEvent : DomainEvent
{
    public Guid UserId { get; init; }
    public Guid RoleId { get; init; }
    public Guid TenantId { get; init; }

    public TenantRoleAssignedDomainEvent(Guid userId, Guid roleId, Guid tenantId)
        : base("TenantRoleAssigned", "User", userId.ToString())
    {
        UserId = userId;
        RoleId = roleId;
        TenantId = tenantId;
    }

    // For deserialization
    [Obsolete("Only for deserialization. Use parameterized constructor.", error: true)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    private TenantRoleAssignedDomainEvent() : base() { }
}
