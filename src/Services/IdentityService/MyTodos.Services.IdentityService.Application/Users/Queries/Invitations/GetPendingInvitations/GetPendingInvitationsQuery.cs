using MyTodos.BuildingBlocks.Application.Abstractions.Queries;

namespace MyTodos.Services.IdentityService.Application.Invitations.Queries.GetPendingInvitations;

/// <summary>
/// Query to get all pending invitations (simulates email inbox).
/// </summary>
public sealed class GetPendingInvitationsQuery : Query<IReadOnlyList<InvitationDto>>
{
    public string? Email { get; init; }
    public Guid? TenantId { get; init; }
}
