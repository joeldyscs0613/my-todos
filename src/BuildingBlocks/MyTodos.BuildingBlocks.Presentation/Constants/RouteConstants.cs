namespace MyTodos.BuildingBlocks.Presentation.Constants;

/// <summary>
/// Constants for API route naming conventions following CQRS patterns.
/// Provides standardized route names for use with CreatedAtRoute and other routing methods.
/// </summary>
/// <remarks>
/// <para>Route names are organized by operation type (Queries vs Commands) to align with CQRS architecture.</para>
/// <para>Route names follow the pattern "{Controller}:{Action}" for consistency and discoverability.</para>
/// <para>Use these constants instead of magic strings to enable compile-time safety and refactoring support.</para>
/// </remarks>
/// <example>
/// <code>
/// // Query endpoint
/// [HttpGet("{id:guid}", Name = RouteNames.Queries.GetById)]
/// public async Task&lt;ActionResult&lt;TodoDto&gt;&gt; GetById(Guid id)
///     => HandleResult(await Mediator.Send(new GetTodoByIdQuery(id)));
///
/// // Command endpoint with reference to query route
/// [HttpPost(Name = RouteNames.Commands.Create)]
/// public async Task&lt;ActionResult&lt;TodoDto&gt;&gt; Create([FromBody] CreateTodoCommand command)
/// {
///     var result = await Mediator.Send(command);
///     return HandleCreatedResult(
///         result,
///         RouteNames.ForController("Todos", RouteNames.Queries.GetById),
///         result.Value?.Id);
/// }
///
/// // Export endpoint (filtered but unpaginated)
/// [HttpGet("export", Name = RouteNames.Queries.Export)]
/// public async Task&lt;ActionResult&lt;IEnumerable&lt;TodoDto&gt;&gt;&gt; Export([FromQuery] TodoFilter filter)
///     => HandleResult(await Mediator.Send(new ExportTodosQuery(filter)));
/// </code>
/// </example>
public static class RouteNames
{
    /// <summary>
    /// Query route names (read operations).
    /// Queries retrieve data without modifying state.
    /// </summary>
    public static class Queries
    {
        /// <summary>
        /// GET by ID endpoint - retrieves a single resource by identifier.
        /// Pattern: GET /api/{controller}/{id}
        /// Example: "Todos:GetById"
        /// </summary>
        public const string GetById = "GetById";

        /// <summary>
        /// GET paginated list endpoint - retrieves a filtered, sorted, paginated list.
        /// Pattern: GET /api/{controller}
        /// Example: "Todos:GetPagedList"
        /// Returns pagination metadata in X-Pagination-* headers.
        /// </summary>
        public const string GetPagedList = "GetPagedList";

        /// <summary>
        /// Export endpoint - retrieves all items matching the same filters as GetPagedList.
        /// Pattern: GET /api/{controller}/export
        /// Example: "Todos:Export"
        /// Uses same filters/specifications as GetPagedList but returns all matching results.
        /// </summary>
        public const string Export = "Export";

        /// <summary>
        /// Get as options endpoint - retrieves minimal data for dropdowns/select lists.
        /// Pattern: GET /api/{controller}/as-options
        /// Example: "Statuses:GetAsOptions"
        /// Returns key-value pairs (Id, Name) optimized for UI selection controls.
        /// </summary>
        public const string GetAsOptions = "GetAsOptions";

        /// <summary>
        /// Create options endpoint - retrieves form initialization data including dropdown options.
        /// Pattern: GET /api/{controller}/create-options
        /// Example: "Todos:CreateOptions"
        /// Returns a DTO containing all dropdown lists and reference data needed for create forms.
        /// Reduces API calls by embedding all form options in a single response.
        /// </summary>
        public const string CreateOptions = "CreateOptions";
    }

    /// <summary>
    /// Command route names (write operations).
    /// Commands modify state and may return the created/updated resource.
    /// </summary>
    public static class Commands
    {
        /// <summary>
        /// POST create endpoint - creates a new resource.
        /// Pattern: POST /api/{controller}
        /// Example: "Todos:Create"
        /// Returns 201 Created with Location header pointing to the GetById route.
        /// </summary>
        public const string Create = "Create";

        /// <summary>
        /// DELETE endpoint - deletes a resource.
        /// Pattern: DELETE /api/{controller}/{id}
        /// Example: "Todos:Delete"
        /// Returns 204 No Content on success.
        /// </summary>
        public const string Delete = "Delete";

        // Note: No generic "Update" command - we will use specific commands instead:
        // - UpdateGeneralInfo, MarkAsComplete, ChangeDueDate, etc.
    }

    /// <summary>
    /// Creates a controller-specific route name.
    /// </summary>
    /// <param name="controllerName">Name of the controller (e.g., "Todos")</param>
    /// <param name="action">Action name (use RouteNames.Queries or RouteNames.Commands constants)</param>
    /// <returns>Formatted route name (e.g., "Todos:GetById")</returns>
    /// <example>
    /// <code>
    /// var routeName = RouteNames.ForController("Todos", RouteNames.Queries.GetById);
    /// // Returns: "Todos:GetById"
    /// </code>
    /// </example>
    public static string ForController(string controllerName, string action)
        => $"{controllerName}:{action}";
}
