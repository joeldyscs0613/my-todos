using MyTodos.BuildingBlocks.Application.Abstractions.Queries;
using MyTodos.Services.IdentityService.Application.Users.Contracts;
using MyTodos.Services.IdentityService.Domain.UserAggregate.Enums;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Users.Queries.Invitations.GetPendingInvitations;

/// <summary>
/// Query to get all pending invitations (simulates email inbox).
/// </summary>
public sealed class GetPendingInvitationsQuery : Query<IReadOnlyList<InvitationResponseDto>>
{
    public string? Email { get; init; }
    public Guid? TenantId { get; init; }
}

/// <summary>
/// Invitation DTO for displaying pending invitations.
/// </summary>
public sealed record InvitationResponseDto
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
    public DateTimeOffset ExpiresAt { get; init; }
    public InvitationStatus Status { get; init; }
}

/// <summary>
/// Handler for getting pending invitations.
/// </summary>
public sealed class GetPendingInvitationsQueryHandler(IUserPagedListReadRepository invitationPagedListReadRepository)
    : QueryHandler<GetPendingInvitationsQuery, IReadOnlyList<InvitationResponseDto>>
{
    public override async Task<Result<IReadOnlyList<InvitationResponseDto>>> Handle(
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

        var dtos = invitations.Select(i => new InvitationResponseDto
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
