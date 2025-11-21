namespace MyTodos.BuildingBlocks.Application.Constants;

/// <summary>
/// Well-known role names used across the platform for authorization.
/// These constants represent the standard roles seeded during application startup.
/// </summary>
public static class WellKnownRoles
{
    // ===== GLOBAL SCOPE =====

    /// <summary>
    /// Platform Administrator - Full platform access across all tenants.
    /// Can manage all tenants, system settings, and platform configuration.
    /// </summary>
    public const string GlobalAdmin = "Global.Admin";

    // ===== TENANT SCOPE =====

    /// <summary>
    /// Organization Administrator - User management within a tenant.
    /// Can manage users within their tenant only.
    /// </summary>
    public const string TenantAdmin = "Tenant.Admin";

    // ===== APP SCOPE =====

    /// <summary>
    /// Application Administrator - Full application access.
    /// Can create/delete projects, manage all tasks, and configure project settings.
    /// </summary>
    public const string AppAdmin = "App.Admin";

    /// <summary>
    /// Application Contributor - Regular user with read/write access.
    /// Can create projects, create/edit/complete tasks, and collaborate on work.
    /// This is the default role for most users.
    /// </summary>
    public const string AppContributor = "App.Contributor";

    /// <summary>
    /// Application Observer - Read-only application access.
    /// Can view projects and tasks but cannot create or edit them.
    /// </summary>
    public const string AppObserver = "App.Observer";
}
