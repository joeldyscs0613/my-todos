using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyTodos.BuildingBlocks.Presentation.Authorization;
using MyTodos.BuildingBlocks.Presentation.Extensions;
using MyTodos.Services.IdentityService.Application.Features.Users.Commands.InviteUser;
using MyTodos.Services.IdentityService.Application.Features.Users.Queries.GetUserDetails;
using MyTodos.Services.IdentityService.Contracts;

namespace MyTodos.Services.IdentityService.Api.Controllers;

/// <summary>
/// Handles user management operations.
/// </summary>
[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly ISender _sender;

    public UsersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get user details by ID.
    /// </summary>
    [HttpGet("{userId:guid}")]
    [HasPermission(Permissions.Users.Read)]
    [ProducesResponseType(typeof(UserDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserDetails(Guid userId, CancellationToken ct)
    {
        var query = new GetUserDetailsQuery(userId);
        var result = await _sender.Send(query, ct);
        return result.ToActionResult();
    }

    /// <summary>
    /// Invite a user to join the system or a tenant.
    /// </summary>
    [HttpPost("invite")]
    [HasPermission(Permissions.Invitations.Create)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> InviteUser([FromBody] InviteUserCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);

        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(GetUserDetails),
                new { userId = result.Value!.Id },
                result.Value);
        }

        return result.ToActionResult();
    }
}
