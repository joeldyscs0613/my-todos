using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using MyTodos.BuildingBlocks.Application.Helpers;
using MyTodos.BuildingBlocks.Presentation.Extensions;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.BuildingBlocks.Presentation.Controllers;

/// <summary>
/// Base controller providing common functionality for API controllers.
/// Provides MediatR integration and helper methods for Result pattern handling.
/// </summary>
/// <remarks>
/// <para><strong>Key Features:</strong></para>
/// <list type="bullet">
/// <item><description>Lazy-loaded IMediator instance (no constructor dependency required)</description></item>
/// <item><description>Result to ActionResult conversion helpers</description></item>
/// <item><description>Pagination header support for PagedList results</description></item>
/// <item><description>RESTful conventions (201 Created with location header)</description></item>
/// <item><description>Convention-based routing: [Route("api/[controller]")]</description></item>
/// </list>
/// <para><strong>Design Principles:</strong></para>
/// <list type="bullet">
/// <item><description>Thin controllers - all business logic in handlers</description></item>
/// <item><description>Consistent error handling via Result pattern</description></item>
/// <item><description>Type-safe helper methods</description></item>
/// <item><description>RESTful HTTP semantics (200, 201, 204, 400, 404, etc.)</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// [ApiController]
/// [Route("api/todos")]
/// public class TodosController : ApiControllerBase
/// {
///     [HttpGet("{id:guid}", Name = "Todos:GetById")]
///     public async Task&lt;ActionResult&lt;TodoResponse&gt;&gt; GetById(Guid id, CancellationToken ct)
///     {
///         var result = await Mediator.Send(new GetTodoByIdQuery(id), ct);
///         return HandleResult(result);
///     }
///
///     [HttpGet]
///     public async Task&lt;ActionResult&lt;IEnumerable&lt;TodoListItemResponse&gt;&gt;&gt; GetList(
///         [FromQuery] TodoFilter filter,
///         CancellationToken ct)
///     {
///         var result = await Mediator.Send(new GetTodosPagedQuery(filter), ct);
///         return HandlePagedResult(result);
///     }
///
///     [HttpPost]
///     public async Task&lt;ActionResult&lt;TodoResponse&gt;&gt; Create(
///         [FromBody] CreateTodoCommand command,
///         CancellationToken ct)
///     {
///         var result = await Mediator.Send(command, ct);
///         return HandleCreatedResult(result, "Todos:GetById", result.Value?.Id);
///     }
/// }
/// </code>
/// </example>
[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private IMediator? _mediator;

    /// <summary>
    /// Gets the MediatR instance for sending commands and queries.
    /// Lazily resolved from HttpContext.RequestServices to avoid constructor dependencies.
    /// </summary>
    protected IMediator Mediator =>
        _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

    /// <summary>
    /// Converts a Result to an ActionResult.
    /// Returns 204 No Content on success, or Problem Details on failure.
    /// </summary>
    /// <param name="result">The command result to convert.</param>
    /// <returns>
    /// - 204 No Content if result is successful
    /// - Problem Details with appropriate status code if result is failed
    /// </returns>
    /// <example>
    /// <code>
    /// [HttpDelete("{id:guid}")]
    /// public async Task&lt;ActionResult&gt; Delete(Guid id, CancellationToken ct)
    /// {
    ///     var result = await Mediator.Send(new DeleteTodoCommand(id), ct);
    ///     return HandleResult(result); // Returns 204 or error
    /// }
    /// </code>
    /// </example>
    protected ActionResult HandleResult(Result result)
        => result.ToActionResult();

    /// <summary>
    /// Converts a Result&lt;TValue&gt; to an ActionResult&lt;TValue&gt;.
    /// Returns 200 OK with value on success, or Problem Details on failure.
    /// </summary>
    /// <typeparam name="TValue">Type of the value returned by the result.</typeparam>
    /// <param name="result">The query/command result to convert.</param>
    /// <returns>
    /// - 200 OK with value if result is successful
    /// - Problem Details with appropriate status code if result is failed
    /// </returns>
    /// <example>
    /// <code>
    /// [HttpGet("{id:guid}")]
    /// public async Task&lt;ActionResult&lt;TodoResponse&gt;&gt; GetById(Guid id, CancellationToken ct)
    /// {
    ///     var result = await Mediator.Send(new GetTodoByIdQuery(id), ct);
    ///     return HandleResult(result); // Returns 200 OK or error
    /// }
    /// </code>
    /// </example>
    protected ActionResult<TValue> HandleResult<TValue>(Result<TValue> result)
        => result.ToActionResult();

    /// <summary>
    /// Converts a Result&lt;TValue&gt; to a 201 Created response with location header.
    /// Returns 201 Created with location header on success, or Problem Details on failure.
    /// </summary>
    /// <typeparam name="TValue">Type of the created resource.</typeparam>
    /// <typeparam name="TId">Type of the resource identifier.</typeparam>
    /// <param name="result">The creation result to convert.</param>
    /// <param name="routeName">Name of the route to the GetById endpoint.</param>
    /// <param name="id">ID of the created resource.</param>
    /// <returns>
    /// - 201 Created with Location header and resource if result is successful
    /// - Problem Details with appropriate status code if result is failed
    /// </returns>
    /// <remarks>
    /// The route name should match the Name property of the GetById endpoint's HttpGet attribute.
    /// Use RouteConstants or define route names as constants for type safety.
    /// </remarks>
    /// <example>
    /// <code>
    /// [HttpPost]
    /// public async Task&lt;ActionResult&lt;TodoResponse&gt;&gt; Create(
    ///     [FromBody] CreateTodoCommand command,
    ///     CancellationToken ct)
    /// {
    ///     var result = await Mediator.Send(command, ct);
    ///     return HandleCreatedResult(result, "Todos:GetById", result.Value?.Id);
    ///     // Returns: 201 Created
    ///     // Location: /api/todos/{id}
    ///     // Body: { ...todo data... }
    /// }
    /// </code>
    /// </example>
    protected ActionResult<TValue> HandleCreatedResult<TValue, TId>(
        Result<TValue> result,
        string routeName,
        TId? id) where TId : struct
    {
        if (result.IsFailure)
            return result.ToProblemDetails();

        return CreatedAtRoute(routeName, new { id }, result.Value);
    }

    /// <summary>
    /// Converts a Result&lt;PagedList&lt;TItem&gt;&gt; to an ActionResult with pagination headers.
    /// Returns 200 OK with items and pagination headers on success, or Problem Details on failure.
    /// </summary>
    /// <typeparam name="TItem">Type of items in the paged list.</typeparam>
    /// <param name="result">The paged query result to convert.</param>
    /// <returns>
    /// - 200 OK with items and X-Pagination-* headers if result is successful
    /// - Problem Details with appropriate status code if result is failed
    /// </returns>
    /// <remarks>
    /// <para>Pagination headers added to response:</para>
    /// <list type="bullet">
    /// <item><description>X-Pagination-CurrentPage: Current page number</description></item>
    /// <item><description>X-Pagination-PageSize: Items per page</description></item>
    /// <item><description>X-Pagination-TotalPages: Total number of pages</description></item>
    /// <item><description>X-Pagination-TotalCount: Total number of items</description></item>
    /// <item><description>X-Pagination-HasPrevious: Whether previous page exists</description></item>
    /// <item><description>X-Pagination-HasNext: Whether next page exists</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// [HttpGet]
    /// public async Task&lt;ActionResult&lt;IEnumerable&lt;TodoListItemResponse&gt;&gt;&gt; GetList(
    ///     [FromQuery] TodoFilter filter,
    ///     CancellationToken ct)
    /// {
    ///     var result = await Mediator.Send(new GetTodosPagedQuery(filter), ct);
    ///     return HandlePagedResult(result);
    ///     // Returns: 200 OK with X-Pagination-* headers
    ///     // Body: [...array of items...]
    /// }
    /// </code>
    /// </example>
    protected ActionResult<IEnumerable<TItem>> HandlePagedResult<TItem>(
        Result<PagedList<TItem>> result)
    {
        if (result.IsFailure)
            return result.ToProblemDetails();

        var pagedList = result.Value;
        Response.AddPaginationHeaders(pagedList.Metadata);

        return Ok(pagedList.AsEnumerable());
    }
}
