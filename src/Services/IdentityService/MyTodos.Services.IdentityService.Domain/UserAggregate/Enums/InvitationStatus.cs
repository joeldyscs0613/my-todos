namespace MyTodos.Services.IdentityService.Domain.UserAggregate.Enums;

/// <summary>
/// Status of a user invitation.
/// </summary>
public enum InvitationStatus
{
    /// <summary>
    /// Invitation is pending acceptance
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Invitation has been accepted and user registered
    /// </summary>
    Accepted = 1,

    /// <summary>
    /// Invitation has expired
    /// </summary>
    Expired = 2,

    /// <summary>
    /// Invitation was cancelled/revoked
    /// </summary>
    Cancelled = 3
}
