namespace MyTodos.BuildingBlocks.Presentation.Constants;

/// <summary>
/// Route name constants for CQRS endpoints. Use with CreatedAtRoute and routing methods.
/// Pattern: "{Controller}:{Action}" (e.g., "Todos:GetById").
/// </summary>
public static class RouteNames
{
    /// <summary>
    /// Query route names (read operations).
    /// </summary>
    public static class Queries
    {
        /// <summary>
        /// GET by ID endpoint. Pattern: GET /api/{controller}/{id}
        /// </summary>
        public const string GetById = "GetById";

        /// <summary>
        /// GET paginated list endpoint. Returns pagination metadata in headers.
        /// </summary>
        public const string GetPagedList = "GetPagedList";

        /// <summary>
        /// Export endpoint. Returns all items matching filters without pagination.
        /// </summary>
        public const string Export = "Export";

        /// <summary>
        /// Get as options endpoint. Returns minimal data for dropdowns.
        /// </summary>
        public const string GetAsOptions = "GetAsOptions";

        /// <summary>
        /// Create options endpoint. Returns form initialization data with dropdown options.
        /// </summary>
        public const string CreateOptions = "CreateOptions";
    }

    /// <summary>
    /// Command route names (write operations).
    /// </summary>
    public static class Commands
    {
        /// <summary>
        /// POST create endpoint. Returns 201 Created with Location header.
        /// </summary>
        public const string Create = "Create";

        /// <summary>
        /// DELETE endpoint. Returns 204 No Content on success.
        /// </summary>
        public const string Delete = "Delete";

        // Note: No generic "Update" command - use specific commands instead
        // (UpdateGeneralInfo, MarkAsComplete, ChangeDueDate, etc.)
    }

    /// <summary>
    /// Creates a controller-specific route name in "{Controller}:{Action}" format.
    /// </summary>
    /// <param name="controllerName">Name of the controller (e.g., "Todos")</param>
    /// <param name="action">Action name from RouteNames.Queries or RouteNames.Commands</param>
    /// <returns>Formatted route name (e.g., "Todos:GetById")</returns>
    public static string ForController(string controllerName, string action)
        => $"{controllerName}:{action}";
}
