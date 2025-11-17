using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyTodos.BuildingBlocks.Presentation.Authorization;
using MyTodos.BuildingBlocks.Presentation.Extensions;
using MyTodos.Services.IdentityService.Application.Features.Tenants.Commands.CreateTenant;
using MyTodos.Services.IdentityService.Contracts;

namespace MyTodos.Services.IdentityService.Api.Controllers;

/// <summary>
/// Handles tenant management operations.
/// </summary>
[ApiController]
[Route("api/tenants")]
[HasPermission(Permissions.Tenants.Create)]
public sealed class TenantsController : ControllerBase
{
    private readonly ISender _sender;

    public TenantsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Create a new tenant (Global Admin only).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);

        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(CreateTenant),
                new { tenantId = result.Value!.Id },
                result.Value);
        }

        return result.ToActionResult();
    }
}
