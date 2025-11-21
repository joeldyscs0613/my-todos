using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyTodos.BuildingBlocks.Application.Helpers;
using MyTodos.BuildingBlocks.Presentation.Authorization;
using MyTodos.BuildingBlocks.Presentation.Controllers;
using MyTodos.BuildingBlocks.Presentation.Extensions;
using MyTodos.Services.IdentityService.Application.Tenants.Commands.ActivateTenant;
using MyTodos.Services.IdentityService.Application.Tenants.Commands.CreateTenant;
using MyTodos.Services.IdentityService.Application.Tenants.Commands.DeactivateTenant;
using MyTodos.Services.IdentityService.Application.Tenants.Commands.DeleteTenant;
using MyTodos.Services.IdentityService.Application.Tenants.Commands.UpdateTenantGeneralInfo;
using MyTodos.Services.IdentityService.Application.Tenants.Queries.GetPagedList;
using MyTodos.Services.IdentityService.Application.Tenants.Queries.GetTenantDetails;
using MyTodos.Services.IdentityService.Contracts;

namespace MyTodos.Services.IdentityService.Api.Controllers;

/// <summary>
/// Handles tenant management operations.
/// </summary>
[ApiController]
[Route("api/tenants")]
public sealed class TenantsController : ApiControllerBase
{
    /// <summary>
    /// Get paginated list of tenants with optional filtering and sorting.
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.Tenants.ViewList)]
    [ProducesResponseType(typeof(PagedList<TenantPagedListResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTenantsList([FromQuery] TenantPagedListFilter filter, CancellationToken ct)
    {
        var query = new GetTenantPagedListQuery(filter);
        var result = await Sender.Send(query, ct);

        return result.ToActionResult();
    }

    /// <summary>
    /// Get tenant details by ID.
    /// </summary>
    [HttpGet("{tenantId:guid}")]
    [HasPermission(Permissions.Tenants.ViewDetails)]
    [ProducesResponseType(typeof(TenantDetailsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTenantDetails(Guid tenantId, CancellationToken ct)
    {
        var query = new GetTenantDetailsQuery(tenantId);
        var result = await Sender.Send(query, ct);

        return result.ToActionResult();
    }

    /// <summary>
    /// Create a new tenant (Global Admin only).
    /// </summary>
    [HttpPost]
    [HasPermission(Permissions.Tenants.Create)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);

        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(GetTenantDetails),
                new { tenantId = result.Value!.Id },
                result.Value);
        }

        return result.ToActionResult();
    }

    /// <summary>
    /// Update tenant general information (name, etc.).
    /// This is a command operation following CQRS principles.
    /// </summary>
    [HttpPut("{tenantId:guid}/general-info")]
    [HasPermission(Permissions.Tenants.ManageDetails)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateTenantGeneralInfo(
        Guid tenantId, [FromBody] UpdateTenantGeneralInfoCommand command, CancellationToken ct)
    {
        // Ensure the tenantId from route matches the command
        if (command.TenantId == Guid.Empty)
        {
            command = new UpdateTenantGeneralInfoCommand(tenantId, command.Name);
        }
        else if (command.TenantId != tenantId)
        {
            return BadRequest("Tenant ID in route does not match the request body");
        }

        var result = await Sender.Send(command, ct);

        return result.ToActionResult();
    }

    /// <summary>
    /// Activate a tenant.
    /// </summary>
    [HttpPut("{tenantId:guid}/activate")]
    [HasPermission(Permissions.Tenants.ManageDetails)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ActivateTenant(Guid tenantId, CancellationToken ct)
    {
        var command = new ActivateTenantCommand(tenantId);
        var result = await Sender.Send(command, ct);

        return result.ToActionResult();
    }

    /// <summary>
    /// Deactivate a tenant.
    /// </summary>
    [HttpPut("{tenantId:guid}/deactivate")]
    [HasPermission(Permissions.Tenants.ManageDetails)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeactivateTenant(Guid tenantId, CancellationToken ct)
    {
        var command = new DeactivateTenantCommand(tenantId);
        var result = await Sender.Send(command, ct);

        return result.ToActionResult();
    }

    /// <summary>
    /// Permanently delete a tenant (Global Admin only).
    /// This is a hard delete operation that removes the tenant from the database.
    /// </summary>
    [HttpDelete("{tenantId:guid}")]
    [HasPermission(Permissions.Tenants.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteTenant(Guid tenantId, CancellationToken ct)
    {
        var command = new DeleteTenantCommand(tenantId);
        var result = await Sender.Send(command, ct);

        return result.ToActionResult();
    }
}
