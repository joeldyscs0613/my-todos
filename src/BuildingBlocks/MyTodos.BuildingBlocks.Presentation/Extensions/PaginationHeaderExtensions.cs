using Microsoft.AspNetCore.Http;
using MyTodos.BuildingBlocks.Application.Helpers;
using MyTodos.BuildingBlocks.Presentation.Constants;

namespace MyTodos.BuildingBlocks.Presentation.Extensions;

/// <summary>
/// Extension methods for adding pagination metadata to HTTP response headers.
/// </summary>
public static class PaginationHeaderExtensions
{
    /// <summary>
    /// Adds pagination metadata headers (X-Pagination-*) to the HTTP response.
    /// Includes CurrentPage, PageSize, TotalPages, TotalCount, HasPrevious, HasNext, SortField, SortDirection.
    /// </summary>
    /// <param name="response">The HTTP response to add headers to.</param>
    /// <param name="metadata">Pagination metadata from a PagedList.</param>
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
    /// Adds pagination metadata headers from a PagedList. Extracts and adds metadata.
    /// </summary>
    /// <typeparam name="T">Type of items in the paged list.</typeparam>
    /// <param name="response">The HTTP response to add headers to.</param>
    /// <param name="pagedList">The paged list containing metadata.</param>
    public static void AddPaginationHeaders<T>(this HttpResponse response, PagedList<T> pagedList)
    {
        ArgumentNullException.ThrowIfNull(pagedList);
        response.AddPaginationHeaders(pagedList.Metadata);
    }
}
