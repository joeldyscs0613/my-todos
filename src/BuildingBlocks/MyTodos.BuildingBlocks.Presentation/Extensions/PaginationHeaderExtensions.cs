using Microsoft.AspNetCore.Http;
using MyTodos.BuildingBlocks.Application.Helpers;
using MyTodos.BuildingBlocks.Presentation.Constants;

namespace MyTodos.BuildingBlocks.Presentation.Extensions;

/// <summary>
/// Extension methods for adding pagination metadata to HTTP response headers.
/// Provides RESTful pagination information following RFC 8288 conventions.
/// </summary>
public static class PaginationHeaderExtensions
{
    /// <summary>
    /// Adds pagination metadata headers to the HTTP response.
    /// Headers follow the X-Pagination-* naming convention.
    /// </summary>
    /// <param name="response">The HTTP response to add headers to.</param>
    /// <param name="metadata">Pagination metadata from a PagedList.</param>
    /// <remarks>
    /// <para>Headers added:</para>
    /// <list type="bullet">
    /// <item><description>X-Pagination-CurrentPage: Current page number (1-indexed)</description></item>
    /// <item><description>X-Pagination-PageSize: Items per page</description></item>
    /// <item><description>X-Pagination-TotalPages: Total number of pages</description></item>
    /// <item><description>X-Pagination-TotalCount: Total number of items</description></item>
    /// <item><description>X-Pagination-HasPrevious: Whether a previous page exists (true/false)</description></item>
    /// <item><description>X-Pagination-HasNext: Whether a next page exists (true/false)</description></item>
    /// <item><description>X-Pagination-SortField: Sort field name (optional)</description></item>
    /// <item><description>X-Pagination-SortDirection: Sort direction (optional)</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var pagedResult = await Mediator.Send(new GetTodosQuery(filter));
    /// Response.AddPaginationHeaders(pagedResult.Metadata);
    /// return Ok(pagedResult);
    /// </code>
    /// </example>
    public static void AddPaginationHeaders(this HttpResponse response, PagedListMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(metadata);

        response.Headers.Append(HeaderConstants.Pagination.CurrentPage,
            metadata.PageNumber.ToString());

        response.Headers.Append(HeaderConstants.Pagination.PageSize,
            metadata.PageSize.ToString());

        response.Headers.Append(HeaderConstants.Pagination.TotalPages,
            metadata.TotalPages.ToString());

        response.Headers.Append(HeaderConstants.Pagination.TotalCount,
            metadata.TotalCount.ToString());

        response.Headers.Append(HeaderConstants.Pagination.HasPrevious,
            metadata.HasPreviousPage.ToString().ToLowerInvariant());

        response.Headers.Append(HeaderConstants.Pagination.HasNext,
            metadata.HasNextPage.ToString().ToLowerInvariant());

        // Add optional sort metadata if present
        if (!string.IsNullOrWhiteSpace(metadata.SortField))
        {
            response.Headers.Append(HeaderConstants.Pagination.SortField, metadata.SortField);
        }

        if (!string.IsNullOrWhiteSpace(metadata.SortDirection))
        {
            response.Headers.Append(HeaderConstants.Pagination.SortDirection,
                metadata.SortDirection.ToLowerInvariant());
        }
    }

    /// <summary>
    /// Adds pagination metadata headers from a PagedList directly.
    /// Convenience method that extracts metadata and adds headers.
    /// </summary>
    /// <typeparam name="T">Type of items in the paged list.</typeparam>
    /// <param name="response">The HTTP response to add headers to.</param>
    /// <param name="pagedList">The paged list containing metadata.</param>
    /// <example>
    /// <code>
    /// var pagedResult = await Mediator.Send(new GetTodosQuery(filter));
    /// Response.AddPaginationHeaders(pagedResult);
    /// return Ok(pagedResult);
    /// </code>
    /// </example>
    public static void AddPaginationHeaders<T>(this HttpResponse response, PagedList<T> pagedList)
    {
        ArgumentNullException.ThrowIfNull(pagedList);
        response.AddPaginationHeaders(pagedList.Metadata);
    }
}
