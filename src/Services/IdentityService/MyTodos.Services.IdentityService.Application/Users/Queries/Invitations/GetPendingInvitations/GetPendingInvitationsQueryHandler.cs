using MyTodos.BuildingBlocks.Application.Abstractions.Queries;
using MyTodos.Services.IdentityService.Application.Users.Contracts;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Users.Queries.Invitations.GetPendingInvitations;

/// <summary>
/// Handler for getting pending invitations.
/// </summary>
public sealed class GetPendingInvitationsQueryHandler(IUserPagedListReadRepository invitationPagedListReadRepository)
    : QueryHandler<GetPendingInvitationsQuery, IReadOnlyList<InvitationDto>>
{
    public override async Task<Result<IReadOnlyList<InvitationDto>>> Handle(
        GetPendingInvitationsQuery request,
        CancellationToken ct)
    {
        IReadOnlyList<Domain.UserAggregate.UserInvitation> invitations;

        // Filter by email if provided (simulates user checking their email for invitations)
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            invitations = await invitationPagedListReadRepository.GetUserInvitationsPendingByEmailAsync(request.Email, ct);
        }
        // Filter by tenant if provided
        else if (request.TenantId.HasValue)
        {
            invitations = await invitationPagedListReadRepository
                .GetUserInvitationsPendingByTenantIdAsync(request.TenantId.Value, ct);
        }
        else
        {
            // No filter - return empty list (require at least one filter)
            invitations = new List<Domain.UserAggregate.UserInvitation>();
        }

        var dtos = invitations.Select(i => new InvitationDto
        {
            Id = i.Id,
            Email = i.Email,
            InvitationToken = i.InvitationToken,
            InvitedByUserId = i.InvitedByUserId,
            InvitedByUsername = i.InvitedByUser.Username,
            TenantId = i.TenantId,
            TenantName = i.Tenant?.Name,
            RoleId = i.RoleId,
            RoleName = i.Role.Name,
            ExpiresAt = i.ExpiresAt,
            Status = i.Status
        }).ToList();

        return Success(dtos);
    }
}
