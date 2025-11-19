using MyTodos.Services.IdentityService.Domain.RoleAggregate;
using MyTodos.Services.IdentityService.Domain.TenantAggregate;
using MyTodos.Services.IdentityService.Domain.UserAggregate.Enums;
using MyTodos.SharedKernel.Abstractions;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Domain.UserAggregate;

/// <summary>
/// Represents an invitation for a user to register and join a tenant.
/// Part of the User aggregate for invitation management.
/// </summary>
public sealed class UserInvitation : Entity<Guid>
{
    /// <summary>
    /// Email address of the invited user
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Unique token for accepting the invitation
    /// </summary>
    public string InvitationToken { get; private set; } = string.Empty;

    /// <summary>
    /// User who sent the invitation
    /// </summary>
    public Guid InvitedByUserId { get; private set; }

    /// <summary>
    /// Tenant the user is being invited to
    /// </summary>
    public Guid? TenantId { get; private set; }

    /// <summary>
    /// Role to assign to the user upon registration
    /// </summary>
    public Guid RoleId { get; private set; }

    /// <summary>
    /// When the invitation expires
    /// </summary>
    public DateTimeOffset ExpiresAt { get; private set; }

    /// <summary>
    /// When the invitation was accepted (null if not accepted)
    /// </summary>
    public DateTimeOffset? AcceptedAt { get; private set; }

    /// <summary>
    /// Current status of the invitation
    /// </summary>
    public InvitationStatus Status { get; private set; }

    // Navigation properties for read operations
    public User InvitedByUser { get; private set; } = null!;
    public Tenant? Tenant { get; private set; }
    public Role Role { get; private set; } = null!;

    /// <summary>
    /// EF Core constructor
    /// </summary>
    [Obsolete("Only for deserialization. Use Create method.", error: true)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    private UserInvitation() : base() { }

    private UserInvitation(Guid id) : base(id) { }

    /// <summary>
    /// Create a new user invitation (internal - only User aggregate can create)
    /// </summary>
    public static Result<UserInvitation> Create(
        Guid invitedByUserId,
        string email,
        Guid roleId,
        Guid? tenantId = null,
        int expirationDays = 7)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email cannot be empty");

        var invitationId = Guid.NewGuid();
        var invitation = new UserInvitation(invitationId)
        {
            Email = email.ToLowerInvariant(),
            InvitationToken = GenerateInvitationToken(),
            InvitedByUserId = invitedByUserId,
            TenantId = tenantId,
            RoleId = roleId,
            ExpiresAt = DateTimeOffsetHelper.UtcNow.AddDays(expirationDays),
            Status = InvitationStatus.Pending
        };

        return invitation;
    }

    /// <summary>
    /// Mark invitation as accepted
    /// </summary>
    public Result MarkAsAccepted()
    {
        if (Status != InvitationStatus.Pending)
            return Result.BadRequest($"Cannot accept invitation with status {Status}");

        if (IsExpired())
            return Result.BadRequest("Cannot accept expired invitation");

        Status = InvitationStatus.Accepted;
        AcceptedAt = DateTimeOffsetHelper.UtcNow;
        
        return Result.Success();
    }

    /// <summary>
    /// Mark invitation as cancelled
    /// </summary>
    public Result Cancel()
    {
        if (Status != InvitationStatus.Pending)
            return Result.BadRequest($"Cannot cancel invitation with status {Status}");

        Status = InvitationStatus.Cancelled;
        
        return Result.Success();
    }

    /// <summary>
    /// Check if invitation is expired
    /// </summary>
    public bool IsExpired()
    {
        return DateTimeOffsetHelper.UtcNow > ExpiresAt;
    }

    /// <summary>
    /// Check if invitation is valid for acceptance
    /// </summary>
    public bool IsValid()
    {
        return Status == InvitationStatus.Pending && !IsExpired();
    }

    private static string GenerateInvitationToken()
    {
        // Generate a cryptographically secure random token
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "")
            .Substring(0, 32);
    }
}
