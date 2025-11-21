using Microsoft.AspNetCore.Mvc;
using MyTodos.BuildingBlocks.Application.Helpers;
using MyTodos.BuildingBlocks.Presentation.Authorization;
using MyTodos.BuildingBlocks.Presentation.Controllers;
using MyTodos.BuildingBlocks.Presentation.Extensions;
using MyTodos.Services.IdentityService.Application.Roles.Commands.CreateRole;
using MyTodos.Services.IdentityService.Application.Roles.Commands.DeleteRole;
using MyTodos.Services.IdentityService.Application.Roles.Commands.UpdateRoleGeneralInfo;
using MyTodos.Services.IdentityService.Application.Roles.Queries.GetPagedList;
using MyTodos.Services.IdentityService.Application.Roles.Queries.GetRoleDetails;
using MyTodos.Services.IdentityService.Contracts;

namespace MyTodos.Services.IdentityService.Api.Controllers;

/// <summary>
/// Handles role management operations.
/// </summary>
[ApiController]
[Route("api/roles")]
public sealed class RolesController : ApiControllerBase
{
    /// <summary>
    /// Get paginated list of roles with optional filtering and sorting.
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.Roles.ViewList)]
    [ProducesResponseType(typeof(PagedList<RolePagedListResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRolesList([FromQuery] RolePagedListFilter filter, CancellationToken ct)
    {
        var query = new GetRoleGetPagedListQuery(filter);
        var result = await Sender.Send(query, ct);

        return result.ToActionResult();
    }

    /// <summary>
    /// Get role details by ID.
    /// </summary>
    [HttpGet("{roleId:guid}")]
    [HasPermission(Permissions.Roles.ViewDetails)]
    [ProducesResponseType(typeof(RoleDetailsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoleDetails(Guid roleId, CancellationToken ct)
    {
        var query = new GetRoleDetailsQuery(roleId);
        var result = await Sender.Send(query, ct);

        return result.ToActionResult();
    }

    /// <summary>
    /// Create a new role (Global Admin only).
    /// </summary>
    [HttpPost]
    [HasPermission(Permissions.Roles.Create)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);

        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(GetRoleDetails),
                new { roleId = result.Value!.Id },
                result.Value);
        }

        return result.ToActionResult();
    }

    /// <summary>
    /// Update role general information (name and description).
    /// Code and Scope are immutable and cannot be updated.
    /// </summary>
    [HttpPut("{roleId:guid}/general-info")]
    [HasPermission(Permissions.Roles.ManageDetails)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateRoleGeneralInfo(
        Guid roleId, [FromBody] UpdateRoleGeneralInfoCommand command, CancellationToken ct)
    {
        // Ensure the roleId from route matches the command
        if (command.RoleId == Guid.Empty)
        {
            command = new UpdateRoleGeneralInfoCommand(roleId, command.Name, command.Description);
        }
        else if (command.RoleId != roleId)
        {
            return BadRequest("Role ID in route does not match the request body");
        }

        var result = await Sender.Send(command, ct);

        return result.ToActionResult();
    }

    /// <summary>
    /// Permanently delete a role (Global Admin only).
    /// This is a hard delete operation that removes the role from the database.
    /// </summary>
    [HttpDelete("{roleId:guid}")]
    [HasPermission(Permissions.Roles.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteRole(Guid roleId, CancellationToken ct)
    {
        var command = new DeleteRoleCommand(roleId);
        var result = await Sender.Send(command, ct);

        return result.ToActionResult();
    }
}
