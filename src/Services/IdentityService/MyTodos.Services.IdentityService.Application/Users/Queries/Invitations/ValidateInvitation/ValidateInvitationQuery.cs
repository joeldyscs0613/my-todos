using MyTodos.BuildingBlocks.Application.Abstractions.Queries;
using MyTodos.Services.IdentityService.Application.Users.Contracts;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Users.Queries.Invitations.ValidateInvitation;

/// <summary>
/// Query to validate an invitation token.
/// </summary>
public sealed class ValidateInvitationQuery : Query<InvitationValidationResponseDto>
{
    public string InvitationToken { get; init; } = string.Empty;

    public ValidateInvitationQuery(string invitationToken)
    {
        InvitationToken = invitationToken;
    }
}

public sealed record InvitationValidationResponseDto
{
    public bool IsValid { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? TenantName { get; init; }
    public string RoleName { get; init; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; init; }
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Handler for validating invitation tokens.
/// </summary>
public sealed class ValidateInvitationQueryHandler(IUserPagedListReadRepository invitationPagedListReadRepository)
    : QueryHandler<ValidateInvitationQuery, InvitationValidationResponseDto>
{
    public override async Task<Result<InvitationValidationResponseDto>> Handle(
        ValidateInvitationQuery request,
        CancellationToken ct)
    {
        var invitation = await invitationPagedListReadRepository.GetUserInvitationByTokenAsync(request.InvitationToken, ct);

        if (invitation == null)
        {
            return Success(new InvitationValidationResponseDto
            {
                IsValid = false,
                ErrorMessage = "Invalid invitation token",
                Email = string.Empty,
                RoleName = string.Empty,
                ExpiresAt = DateTimeOffset.MinValue
            });
        }

        var isValid = invitation.IsValid();
        var errorMessage = !isValid
            ? (invitation.IsExpired() ? "Invitation has expired" : $"Invitation is {invitation.Status}")
            : null;

        var dto = new InvitationValidationResponseDto
        {
            IsValid = isValid,
            Email = invitation.Email,
            TenantName = invitation.Tenant?.Name,
            RoleName = invitation.Role.Name,
            ExpiresAt = invitation.ExpiresAt,
            ErrorMessage = errorMessage
        };

        return Success(dto);
    }
}
