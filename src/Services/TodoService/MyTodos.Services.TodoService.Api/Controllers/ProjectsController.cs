using Microsoft.AspNetCore.Mvc;
using MyTodos.BuildingBlocks.Application.Contracts.Commands;
using MyTodos.BuildingBlocks.Application.Helpers;
using MyTodos.BuildingBlocks.Presentation.Controllers;
using MyTodos.BuildingBlocks.Presentation.Extensions;
using MyTodos.Services.TodoService.Application.Projects.Commands.ChangeProjectAssignee;
using MyTodos.Services.TodoService.Application.Projects.Commands.CreateProject;
using MyTodos.Services.TodoService.Application.Projects.Queries.GetPagedList;
using MyTodos.Services.TodoService.Application.Projects.Queries.GetProjectDetails;

namespace MyTodos.Services.TodoService.Api.Controllers;

/// <summary>
/// Handles project management operations.
/// </summary>
[ApiController]
[Route("api/projects")]
public sealed class ProjectsController : ApiControllerBase
{
    /// <summary>
    /// Get paginated list of projects with optional filtering and sorting.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedList<ProjectPagedListResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProjectsList([FromQuery] ProjectPagedListFilter filter, CancellationToken ct)
    {
        var query = new ProjectGetPagedListQuery(filter);
        var result = await Sender.Send(query, ct);

        return result.ToActionResult();
    }

    /// <summary>
    /// Get project details by ID.
    /// </summary>
    [HttpGet("{projectId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProjectById(Guid projectId, CancellationToken ct)
    {
        var query = new GetProjectDetailsQuery(projectId);
        var result = await Sender.Send(query, ct);

        return result.ToActionResult();
    }

    /// <summary>
    /// Create a new project.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);

        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(GetProjectById),
                new { projectId = result.Value!.Id },
                result.Value);
        }

        return result.ToActionResult();
    }

    /// <summary>
    /// Change project assignee.
    /// </summary>
    [HttpPut("{projectId:guid}/assignee")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ChangeProjectAssignee(
        Guid projectId,
        [FromBody] ChangeProjectAssigneeRequest request,
        CancellationToken ct)
    {
        var command = new ChangeProjectAssigneeCommand
        {
            ProjectId = projectId,
            AssigneeUserId = request.AssigneeUserId
        };

        var result = await Sender.Send(command, ct);

        return result.IsSuccess
            ? NoContent()
            : result.ToActionResult();
    }
}

/// <summary>
/// Request body for changing project assignee
/// </summary>
public sealed record ChangeProjectAssigneeRequest(Guid? AssigneeUserId);
