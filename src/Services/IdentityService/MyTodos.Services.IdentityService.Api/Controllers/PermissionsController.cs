using Microsoft.AspNetCore.Mvc;
using MyTodos.BuildingBlocks.Application.Helpers;
using MyTodos.BuildingBlocks.Presentation.Authorization;
using MyTodos.BuildingBlocks.Presentation.Controllers;
using MyTodos.BuildingBlocks.Presentation.Extensions;
using MyTodos.Services.IdentityService.Application.Permissions.Commands.CreatePermission;
using MyTodos.Services.IdentityService.Application.Permissions.Commands.DeletePermission;
using MyTodos.Services.IdentityService.Application.Permissions.Commands.UpdatePermissionGeneralInfo;
using MyTodos.Services.IdentityService.Application.Permissions.Queries.GetPagedList;
using MyTodos.Services.IdentityService.Application.Permissions.Queries.GetPermissionDetails;
using MyTodos.Services.IdentityService.Contracts;

namespace MyTodos.Services.IdentityService.Api.Controllers;

/// <summary>
/// Handles permission management operations.
/// </summary>
[ApiController]
[Route("api/permissions")]
public sealed class PermissionsController : ApiControllerBase
{
    /// <summary>
    /// Get paginated list of permissions with optional filtering and sorting.
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.PermissionManagement.ViewList)]
    [ProducesResponseType(typeof(PagedList<PermissionPagedListResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPermissionsList(
        [FromQuery] PermissionPagedListFilter filter, CancellationToken ct)
    {
        var query = new GetPermissionPagedListQuery(filter);
        var result = await Sender.Send(query, ct);

        return result.ToActionResult();
    }

    /// <summary>
    /// Get permission details by ID.
    /// </summary>
    [HttpGet("{permissionId:guid}")]
    [HasPermission(Permissions.PermissionManagement.ViewDetails)]
    [ProducesResponseType(typeof(PermissionDetailsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPermissionDetails(Guid permissionId, CancellationToken ct)
    {
        var query = new GetPermissionDetailsQuery(permissionId);
        var result = await Sender.Send(query, ct);

        return result.ToActionResult();
    }

    /// <summary>
    /// Create a new permission (Global Admin only).
    /// </summary>
    [HttpPost]
    [HasPermission(Permissions.PermissionManagement.Create)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);

        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(GetPermissionDetails),
                new { permissionId = result.Value!.Id },
                result.Value);
        }

        return result.ToActionResult();
    }

    /// <summary>
    /// Update permission general information (name and description).
    /// Code is immutable and cannot be updated.
    /// </summary>
    [HttpPut("{permissionId:guid}/general-info")]
    [HasPermission(Permissions.PermissionManagement.ManageDetails)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdatePermissionGeneralInfo(Guid permissionId, 
        [FromBody] UpdatePermissionGeneralInfoCommand command, CancellationToken ct)
    {
        // Ensure the permissionId from route matches the command
        if (command.PermissionId == Guid.Empty)
        {
            command = new UpdatePermissionGeneralInfoCommand(permissionId, command.Name, command.Description);
        }
        else if (command.PermissionId != permissionId)
        {
            return BadRequest("Permission ID in route does not match the request body");
        }

        var result = await Sender.Send(command, ct);

        return result.ToActionResult();
    }

    /// <summary>
    /// Permanently delete a permission (Global Admin only).
    /// This is a hard delete operation that removes the permission from the database.
    /// </summary>
    [HttpDelete("{permissionId:guid}")]
    [HasPermission(Permissions.PermissionManagement.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeletePermission(Guid permissionId, CancellationToken ct)
    {
        var command = new DeletePermissionCommand(permissionId);
        var result = await Sender.Send(command, ct);

        return result.ToActionResult();
    }
}
