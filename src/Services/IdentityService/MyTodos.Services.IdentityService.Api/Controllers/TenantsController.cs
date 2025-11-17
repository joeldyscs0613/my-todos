using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyTodos.BuildingBlocks.Presentation.Authorization;
using MyTodos.BuildingBlocks.Presentation.Controllers;
using MyTodos.BuildingBlocks.Presentation.Extensions;
using MyTodos.Services.IdentityService.Application.Tenants.Commands.CreateTenant;
using MyTodos.Services.IdentityService.Contracts;

namespace MyTodos.Services.IdentityService.Api.Controllers;

/// <summary>
/// Handles tenant management operations.
/// </summary>
[ApiController]
[Route("api/tenants")]
public sealed class TenantsController : ApiControllerBase
{
    public TenantsController()
    {
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
                nameof(CreateTenant),
                new { tenantId = result.Value!.Id },
                result.Value);
        }

        return result.ToActionResult();
    }
}
