using MyTodos.BuildingBlocks.Application.Abstractions.Queries;

namespace MyTodos.Services.IdentityService.Application.Invitations.Queries.ValidateInvitation;

/// <summary>
/// Query to validate an invitation token.
/// </summary>
public sealed class ValidateInvitationQuery : Query<InvitationValidationDto>
{
    public string InvitationToken { get; init; } = string.Empty;

    public ValidateInvitationQuery(string invitationToken)
    {
        InvitationToken = invitationToken;
    }
}

public sealed record InvitationValidationDto
{
    public bool IsValid { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? TenantName { get; init; }
    public string RoleName { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public string? ErrorMessage { get; init; }
}
