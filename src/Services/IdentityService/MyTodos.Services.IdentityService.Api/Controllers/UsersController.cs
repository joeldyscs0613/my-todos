using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyTodos.BuildingBlocks.Application.Helpers;
using MyTodos.BuildingBlocks.Presentation.Authorization;
using MyTodos.BuildingBlocks.Presentation.Controllers;
using MyTodos.BuildingBlocks.Presentation.Extensions;
using MyTodos.Services.IdentityService.Application.Users.Commands.ActivateUser;
using MyTodos.Services.IdentityService.Application.Users.Commands.CreateUser;
using MyTodos.Services.IdentityService.Application.Users.Commands.DeactivateUser;
using MyTodos.Services.IdentityService.Application.Users.Commands.DeleteUser;
using MyTodos.Services.IdentityService.Application.Users.Commands.UpdateUserGeneralInfo;
using MyTodos.Services.IdentityService.Application.Users.Queries.GetCreateUserOptions;
using MyTodos.Services.IdentityService.Application.Users.Queries.GetPagedList;
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
    /// Get paginated list of users with optional filtering and sorting.
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.Users.ViewList)]
    [ProducesResponseType(typeof(PagedList<UserPagedListResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUsersList([FromQuery] UserPagedListFilter filter, CancellationToken ct)
    {
        var query = new UserGetPagedListQuery(filter);
        var result = await Sender.Send(query, ct);

        return result.ToActionResult();
    }

    /// <summary>
    /// Get user details by ID.
    /// </summary>
    [HttpGet("{userId:guid}")]
    [HasPermission(Permissions.Users.ViewDetails)]
    [ProducesResponseType(typeof(UserDetailsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserDetails(Guid userId, CancellationToken ct)
    {
        var query = new GetUserDetailsQuery(userId);
        var result = await Sender.Send(query, ct);

        return result.ToActionResult();
    }

    /// <summary>
    /// Get dropdown options for user creation form.
    /// Returns available tenants, roles, and scopes based on current user's permissions.
    /// </summary>
    [HttpGet("create-options")]
    [HasPermission(Permissions.Users.Create)]
    [ProducesResponseType(typeof(CreateUserOptionsResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCreateUserOptions(CancellationToken ct)
    {
        var query = new GetCreateUserOptionsQuery();
        var result = await Sender.Send(query, ct);

        return result.ToActionResult();
    }

    /// <summary>
    /// Create a user directly with default credentials.
    /// Username is derived from email (part before @) and password is set to "Password@123!".
    /// </summary>
    [HttpPost]
    [HasPermission(Permissions.Users.Create)]
    [ProducesResponseType(typeof(CreateUserResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);

        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(GetUserDetails),
                new { userId = result.Value!.UserId },
                result.Value);
        }

        return result.ToActionResult();
    }

    /// <summary>
    /// Activate a user account.
    /// Global.Admin can activate any user, Tenant.Admin can only activate users in their tenant.
    /// </summary>
    [HttpPut("{userId:guid}/activate")]
    [HasPermission(Permissions.Users.ManageDetails)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateUser(Guid userId, CancellationToken ct)
    {
        var command = new ActivateUserCommand(userId);
        var result = await Sender.Send(command, ct);

        return result.ToActionResult();
    }

    /// <summary>
    /// Deactivate a user account.
    /// Global.Admin can deactivate any user, Tenant.Admin can only deactivate users in their tenant.
    /// </summary>
    [HttpPut("{userId:guid}/deactivate")]
    [HasPermission(Permissions.Users.ManageDetails)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateUser(Guid userId, CancellationToken ct)
    {
        var command = new DeactivateUserCommand(userId);
        var result = await Sender.Send(command, ct);

        return result.ToActionResult();
    }

    /// <summary>
    /// Update user's general information (first name, last name).
    /// Global.Admin can update any user, Tenant.Admin can only update users in their tenant.
    /// </summary>
    [HttpPut("{userId:guid}/general-info")]
    [HasPermission(Permissions.Users.ManageDetails)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserGeneralInfo(Guid userId, [FromBody] UpdateUserGeneralInfoCommand command, CancellationToken ct)
    {
        // Set the userId from the route parameter
        var commandWithUserId = new UpdateUserGeneralInfoCommand(userId, command.FirstName, command.LastName);
        var result = await Sender.Send(commandWithUserId, ct);

        return result.ToActionResult();
    }

    /// <summary>
    /// Permanently delete a user from the database.
    /// Only Global.Admin can delete users. Tenant.Admin should use deactivate instead.
    /// </summary>
    [HttpDelete("{userId:guid}")]
    [HasPermission(Permissions.Users.ManageDetails)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteUser(Guid userId, CancellationToken ct)
    {
        var command = new DeleteUserCommand(userId);
        var result = await Sender.Send(command, ct);

        return result.ToActionResult();
    }
}
