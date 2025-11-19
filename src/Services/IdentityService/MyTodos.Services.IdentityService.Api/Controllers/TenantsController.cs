using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyTodos.BuildingBlocks.Application.Helpers;
using MyTodos.BuildingBlocks.Presentation.Authorization;
using MyTodos.BuildingBlocks.Presentation.Controllers;
using MyTodos.BuildingBlocks.Presentation.Extensions;
using MyTodos.Services.IdentityService.Application.Tenants.Commands.CreateTenant;
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
        var query = new TenantPagedListQuery(filter);
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
}
