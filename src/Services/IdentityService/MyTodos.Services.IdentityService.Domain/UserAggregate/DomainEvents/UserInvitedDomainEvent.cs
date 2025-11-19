using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.Services.IdentityService.Domain.UserAggregate.DomainEvents;

/// <summary>
/// Domain event raised when a user is invited to join the system.
/// This will be mapped to an integration event to simulate email notification.
/// </summary>
public sealed record UserInvitedDomainEvent : DomainEvent
{
    public Guid InvitationId { get; init; }
    public string Email { get; init; }
    public Guid InvitedByUserId { get; init; }
    public Guid? TenantId { get; init; }
    public Guid RoleId { get; init; }
    public string InvitationToken { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }

    public UserInvitedDomainEvent(
        Guid invitationId,
        string email,
        Guid invitedByUserId,
        Guid? tenantId,
        Guid roleId,
        string invitationToken,
        DateTimeOffset expiresAt)
        : base("UserInvited", "UserInvitation", invitationId.ToString())
    {
        InvitationId = invitationId;
        Email = email;
        InvitedByUserId = invitedByUserId;
        TenantId = tenantId;
        RoleId = roleId;
        InvitationToken = invitationToken;
        ExpiresAt = expiresAt;
    }
}
