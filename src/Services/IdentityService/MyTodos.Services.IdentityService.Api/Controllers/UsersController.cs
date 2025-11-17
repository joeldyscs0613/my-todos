using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyTodos.BuildingBlocks.Presentation.Authorization;
using MyTodos.BuildingBlocks.Presentation.Controllers;
using MyTodos.BuildingBlocks.Presentation.Extensions;
using MyTodos.Services.IdentityService.Application.Users.Commands.InviteUser;
using MyTodos.Services.IdentityService.Application.Users.Queries.GetUserDetails;
using MyTodos.Services.IdentityService.Contracts;

namespace MyTodos.Services.IdentityService.Api.Controllers;

/// <summary>
/// Handles user management operations.
/// </summary>
[ApiController]
[Route("api/users")]
public sealed class UsersController() : ApiControllerBase
{
    /// <summary>
    /// Get user details by ID.
    /// </summary>
    [HttpGet("{userId:guid}")]
    [HasPermission(Permissions.Users.ViewDetails)]
    [ProducesResponseType(typeof(UserDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserDetails(Guid userId, CancellationToken ct)
    {
        var query = new GetUserDetailsQuery(userId);
        var result = await Sender.Send(query, ct);
        
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
        var result = await Sender.Send(command, ct);

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
