using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyTodos.BuildingBlocks.Presentation.Authorization;
using MyTodos.BuildingBlocks.Presentation.Controllers;
using MyTodos.BuildingBlocks.Presentation.Extensions;
using MyTodos.Services.IdentityService.Application.Invitations.Queries.GetPendingInvitations;
using MyTodos.Services.IdentityService.Application.Invitations.Queries.ValidateInvitation;
using MyTodos.Services.IdentityService.Contracts;

namespace MyTodos.Services.IdentityService.Api.Controllers;

/// <summary>
/// Handles invitation-related operations.
/// </summary>
[ApiController]
[Route("api/users/invitations")]
public sealed class InvitationsController : ApiControllerBase
{
    public InvitationsController()
    {
    }

    /// <summary>
    /// Get pending invitations (simulates email inbox).
    /// </summary>
    [HttpGet("pending")]
    [HasPermission(Permissions.Invitations.ViewList)]
    [ProducesResponseType(typeof(IReadOnlyList<InvitationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingInvitations(
        [FromQuery] string? email,
        [FromQuery] Guid? tenantId,
        CancellationToken ct)
    {
        var query = new GetPendingInvitationsQuery { Email = email, TenantId = tenantId };
        var result = await Sender.Send(query, ct);
        
        return result.ToActionResult();
    }

    /// <summary>
    /// Validate an invitation token before registration.
    /// </summary>
    [HttpGet("{token}/validate")]
    [ProducesResponseType(typeof(InvitationValidationDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateInvitation(string token, CancellationToken ct)
    {
        var query = new ValidateInvitationQuery(token);
        var result = await Sender.Send(query, ct);
        
        return result.ToActionResult();
    }
}
