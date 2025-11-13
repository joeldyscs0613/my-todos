namespace MyTodos.BuildingBlocks.Presentation.Constants;

/// <summary>
/// Constants for HTTP response header names used in the application.
/// Provides type-safe access to standard headers.
/// </summary>
public static class HeaderConstants
{
    /// <summary>
    /// Pagination metadata header names following RFC 8288 (Web Linking) conventions.
    /// These headers provide pagination information for list endpoints.
    /// </summary>
    /// <remarks>
    /// Headers are prefixed with "X-Pagination-" to avoid conflicts with standard headers.
    /// Values should be added to response headers for any endpoint returning PagedList.
    /// </remarks>
    public static class Pagination
    {
        /// <summary>
        /// Current page number (1-indexed).
        /// Example: X-Pagination-CurrentPage: 2
        /// </summary>
        public const string CurrentPage = "X-Pagination-CurrentPage";

        /// <summary>
        /// Number of items per page.
        /// Example: X-Pagination-PageSize: 10
        /// </summary>
        public const string PageSize = "X-Pagination-PageSize";

        /// <summary>
        /// Total number of pages available.
        /// Example: X-Pagination-TotalPages: 15
        /// </summary>
        public const string TotalPages = "X-Pagination-TotalPages";

        /// <summary>
        /// Total number of items across all pages.
        /// Example: X-Pagination-TotalCount: 143
        /// </summary>
        public const string TotalCount = "X-Pagination-TotalCount";

        /// <summary>
        /// Indicates if a previous page exists (true/false).
        /// Example: X-Pagination-HasPrevious: true
        /// </summary>
        public const string HasPrevious = "X-Pagination-HasPrevious";

        /// <summary>
        /// Indicates if a next page exists (true/false).
        /// Example: X-Pagination-HasNext: true
        /// </summary>
        public const string HasNext = "X-Pagination-HasNext";

        /// <summary>
        /// Field used for sorting (optional).
        /// Example: X-Pagination-SortField: CreatedAt
        /// </summary>
        public const string SortField = "X-Pagination-SortField";

        /// <summary>
        /// Sort direction: "asc" or "desc" (optional).
        /// Example: X-Pagination-SortDirection: desc
        /// </summary>
        public const string SortDirection = "X-Pagination-SortDirection";
    }
}
