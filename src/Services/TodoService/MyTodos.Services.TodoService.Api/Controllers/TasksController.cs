using Microsoft.AspNetCore.Mvc;
using MyTodos.BuildingBlocks.Application.Contracts.Commands;
using MyTodos.BuildingBlocks.Application.Helpers;
using MyTodos.BuildingBlocks.Presentation.Controllers;
using MyTodos.BuildingBlocks.Presentation.Extensions;
using MyTodos.Services.TodoService.Application.Tasks.Commands.ChangeTaskAssignee;
using MyTodos.Services.TodoService.Application.Tasks.Commands.CreateTask;
using MyTodos.Services.TodoService.Application.Tasks.Queries.GetPagedList;
using MyTodos.Services.TodoService.Application.Tasks.Queries.GetTaskDetails;

namespace MyTodos.Services.TodoService.Api.Controllers;

/// <summary>
/// Handles task management operations.
/// </summary>
[ApiController]
[Route("api/tasks")]
public sealed class TasksController : ApiControllerBase
{
    /// <summary>
    /// Get paginated list of tasks with optional filtering and sorting.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedList<TaskPagedListResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTasksList([FromQuery] TaskPagedListFilter filter, CancellationToken ct)
    {
        var query = new TaskPagedListQuery(filter);
        var result = await Sender.Send(query, ct);

        return result.ToActionResult();
    }

    /// <summary>
    /// Get task details by ID.
    /// </summary>
    [HttpGet("{taskId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaskById(Guid taskId, CancellationToken ct)
    {
        var query = new GetTaskDetailsQuery(taskId);
        var result = await Sender.Send(query, ct);

        return result.ToActionResult();
    }

    /// <summary>
    /// Create a new task.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);

        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(GetTaskById),
                new { taskId = result.Value!.Id },
                result.Value);
        }

        return result.ToActionResult();
    }

    /// <summary>
    /// Change task assignee.
    /// </summary>
    [HttpPut("{taskId:guid}/assignee")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ChangeTaskAssignee(
        Guid taskId,
        [FromBody] ChangeTaskAssigneeRequest request,
        CancellationToken ct)
    {
        var command = new ChangeTaskAssigneeCommand
        {
            TaskId = taskId,
            AssigneeUserId = request.AssigneeUserId
        };

        var result = await Sender.Send(command, ct);

        return result.IsSuccess
            ? NoContent()
            : result.ToActionResult();
    }
}

/// <summary>
/// Request body for changing task assignee
/// </summary>
public sealed record ChangeTaskAssigneeRequest(Guid? AssigneeUserId);
