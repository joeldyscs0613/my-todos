using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using MyTodos.BuildingBlocks.Application.Helpers;
using MyTodos.BuildingBlocks.Presentation.Extensions;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.BuildingBlocks.Presentation.Controllers;

/// <summary>
/// Base controller providing MediatR integration and Result pattern conversion helpers.
/// Supports RESTful conventions with pagination headers and location headers for created resources.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private IMediator? _mediator;

    /// <summary>
    /// Gets the MediatR instance for sending commands and queries.
    /// </summary>
    protected IMediator Mediator =>
        _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

    /// <summary>
    /// Converts a Result to an ActionResult. Returns 204 No Content on success, or Problem Details on failure.
    /// </summary>
    protected ActionResult HandleResult(Result result)
        => result.ToActionResult();

    /// <summary>
    /// Converts a Result&lt;TValue&gt; to an ActionResult&lt;TValue&gt;. Returns 200 OK with value on success, or Problem Details on failure.
    /// </summary>
    protected ActionResult<TValue> HandleResult<TValue>(Result<TValue> result)
        => result.ToActionResult();

    /// <summary>
    /// Converts a Result&lt;TValue&gt; to a 201 Created response with location header.
    /// Returns 201 Created with location header on success, or Problem Details on failure.
    /// </summary>
    /// <param name="routeName">Name of the route to the GetById endpoint (must match HttpGet attribute Name property).</param>
    /// <remarks>
    /// Use RouteConstants or define route names as constants for type safety.
    /// </remarks>
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
    /// Returns 200 OK with items and X-Pagination-* headers on success, or Problem Details on failure.
    /// </summary>
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
