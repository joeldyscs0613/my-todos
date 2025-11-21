namespace MyTodos.Services.IdentityService.Contracts;

/// <summary>
/// Well-known permission constants for easy reference across services.
/// Import this in TodoService, ClientApp, etc. to check permissions without hardcoding strings.
/// These are a subset of the full Permissions class, containing only the most commonly checked permissions.
/// </summary>
public static class WellKnownPermissions
{
    /// <summary>
    /// Super admin wildcard - grants access to all permissions across all resources.
    /// </summary>
    public const string All = "*";

    // ===== IDENTITY SERVICE PERMISSIONS =====

    /// <summary>
    /// User management permissions
    /// </summary>
    public static class Users
    {
        public const string Create = "users/user/create";
        public const string Delete = "users/user/delete";
        public const string ViewList = "users/list/view";
        public const string ViewDetails = "users/details/view";
        public const string ManageDetails = "users/details/manage";
        public const string AssignRole = "users/roles/assign";
        public const string RevokeRole = "users/roles/revoke";
    }

    /// <summary>
    /// Role management permissions
    /// </summary>
    public static class Roles
    {
        public const string Create = "roles/role/create";
        public const string Delete = "roles/role/delete";
        public const string ViewList = "roles/list/view";
        public const string ViewDetails = "roles/details/view";
        public const string AssignPermission = "roles/permissions/assign";
        public const string RevokePermission = "roles/permissions/revoke";
    }

    /// <summary>
    /// Tenant management permissions
    /// </summary>
    public static class Tenants
    {
        public const string Create = "tenants/tenant/create";
        public const string Delete = "tenants/tenant/delete";
        public const string ViewList = "tenants/list/view";
        public const string ViewDetails = "tenants/details/view";
        public const string ManageDetails = "tenants/details/manage";
        public const string ManageSettings = "tenants/settings/manage";
    }

    // ===== TODO SERVICE PERMISSIONS =====

    /// <summary>
    /// Project management permissions (App scope)
    /// </summary>
    public static class Projects
    {
        public const string Create = "projects/project/create";
        public const string Delete = "projects/project/delete";
        public const string ViewList = "projects/list/view";
        public const string ViewDetails = "projects/details/view";
        public const string ManageDetails = "projects/details/manage";
        public const string ManageSettings = "projects/settings/manage";
        public const string Archive = "projects/status/archive";
        public const string Restore = "projects/status/restore";
    }

    /// <summary>
    /// Task management permissions (App scope)
    /// </summary>
    public static class Tasks
    {
        public const string Create = "tasks/task/create";
        public const string Delete = "tasks/task/delete";
        public const string ViewList = "tasks/list/view";
        public const string ViewDetails = "tasks/details/view";
        public const string ManageDetails = "tasks/details/manage";
        public const string Assign = "tasks/assign";
        public const string Complete = "tasks/status/complete";
        public const string Reopen = "tasks/status/reopen";
    }
}
