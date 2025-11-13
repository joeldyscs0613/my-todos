namespace MyTodos.BuildingBlocks.Presentation.Constants;

/// <summary>
/// HTTP response header name constants for pagination and other metadata.
/// </summary>
public static class HeaderConstants
{
    /// <summary>
    /// Pagination metadata header names (X-Pagination-* prefix).
    /// </summary>
    public static class Pagination
    {
        /// <summary>Current page number (1-indexed).</summary>
        public const string CurrentPage = "X-Pagination-CurrentPage";

        /// <summary>Number of items per page.</summary>
        public const string PageSize = "X-Pagination-PageSize";

        /// <summary>Total number of pages available.</summary>
        public const string TotalPages = "X-Pagination-TotalPages";

        /// <summary>Total number of items across all pages.</summary>
        public const string TotalCount = "X-Pagination-TotalCount";

        /// <summary>Indicates if a previous page exists (true/false).</summary>
        public const string HasPrevious = "X-Pagination-HasPrevious";

        /// <summary>Indicates if a next page exists (true/false).</summary>
        public const string HasNext = "X-Pagination-HasNext";

        /// <summary>Field used for sorting (optional).</summary>
        public const string SortField = "X-Pagination-SortField";

        /// <summary>Sort direction: "asc" or "desc" (optional).</summary>
        public const string SortDirection = "X-Pagination-SortDirection";
    }
}
