using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.Services.IdentityService.Domain.UserAggregate.DomainEvents;

/// <summary>
/// Domain event raised when a global role is assigned to a user
/// </summary>
public sealed record GlobalRoleAssignedDomainEvent : DomainEvent
{
    public Guid UserId { get; init; }
    public Guid RoleId { get; init; }

    public GlobalRoleAssignedDomainEvent(Guid userId, Guid roleId)
        : base("GlobalRoleAssigned", "User", userId.ToString())
    {
        UserId = userId;
        RoleId = roleId;
    }

    // For deserialization
    [Obsolete("Only for deserialization. Use parameterized constructor.", error: true)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    private GlobalRoleAssignedDomainEvent() : base() { }
}
