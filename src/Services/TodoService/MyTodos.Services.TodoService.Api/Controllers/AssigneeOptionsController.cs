using Microsoft.AspNetCore.Mvc;
using MyTodos.BuildingBlocks.Presentation.Controllers;
using MyTodos.BuildingBlocks.Presentation.Extensions;
using MyTodos.Services.TodoService.Application.Shared.Queries.GetAssigneeOptions;

namespace MyTodos.Services.TodoService.Api.Controllers;

/// <summary>
/// Provides user options for task/project assignment dropdowns
/// </summary>
[ApiController]
[Route("api/assignee-options")]
public sealed class AssigneeOptionsController : ApiControllerBase
{
    /// <summary>
    /// Get list of users in current tenant for assignment dropdown
    /// </summary>
    /// <remarks>
    /// Returns users from the current user's tenant based on JWT token.
    /// No parameters needed - tenant ID is extracted from the authenticated user's token.
    /// </remarks>
    /// <returns>List of users available for assignment</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var query = new GetAssigneeOptionsQuery();
        var result = await Sender.Send(query, ct);

        return result.ToActionResult();
    }
}
