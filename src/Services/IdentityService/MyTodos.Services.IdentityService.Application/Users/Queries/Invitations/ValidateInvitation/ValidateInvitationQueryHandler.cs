using MyTodos.BuildingBlocks.Application.Abstractions.Queries;
using MyTodos.Services.IdentityService.Application.Users.Contracts;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Users.Queries.Invitations.ValidateInvitation;

/// <summary>
/// Handler for validating invitation tokens.
/// </summary>
public sealed class ValidateInvitationQueryHandler(IUserPagedListReadRepository invitationPagedListReadRepository)
    : QueryHandler<ValidateInvitationQuery, InvitationValidationDto>
{
    public override async Task<Result<InvitationValidationDto>> Handle(
        ValidateInvitationQuery request,
        CancellationToken ct)
    {
        var invitation = await invitationPagedListReadRepository.GetUserInvitationByTokenAsync(request.InvitationToken, ct);

        if (invitation == null)
        {
            return Success(new InvitationValidationDto
            {
                IsValid = false,
                ErrorMessage = "Invalid invitation token",
                Email = string.Empty,
                RoleName = string.Empty,
                ExpiresAt = DateTime.MinValue
            });
        }

        var isValid = invitation.IsValid();
        var errorMessage = !isValid
            ? (invitation.IsExpired() ? "Invitation has expired" : $"Invitation is {invitation.Status}")
            : null;

        var dto = new InvitationValidationDto
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
