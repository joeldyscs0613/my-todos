using MyTodos.Services.IdentityService.Domain.UserAggregate.Enums;

namespace MyTodos.Services.IdentityService.Application.Invitations.Queries.GetPendingInvitations;

/// <summary>
/// Invitation DTO for displaying pending invitations.
/// </summary>
public sealed record InvitationDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string InvitationToken { get; init; } = string.Empty;
    public Guid InvitedByUserId { get; init; }
    public string InvitedByUsername { get; init; } = string.Empty;
    public Guid? TenantId { get; init; }
    public string? TenantName { get; init; }
    public Guid RoleId { get; init; }
    public string RoleName { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public InvitationStatus Status { get; init; }
}
