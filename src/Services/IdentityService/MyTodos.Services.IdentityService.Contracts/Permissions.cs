namespace MyTodos.Services.IdentityService.Contracts;

/// <summary>
/// Centralized permission constants for the IdentityService.
/// These constants can be imported by other services (TodoService, etc.) to avoid hard-coding permission strings.
/// </summary>
/// <remarks>
/// Permission naming convention: {resource}/{context}/{action}
/// Examples: users/user/create, users/details/view, users/list/export
///
/// Three-level hierarchy:
/// - Resource: The domain resource (users, roles, tenants, etc.)
/// - Context: The aspect or view of the resource (user, list, details, security, etc.)
/// - Action: The operation being performed (view, manage, create, delete, etc.)
///
/// Wildcard support:
/// - "*" = Super admin (all permissions)
/// - "users/*" = All user permissions
/// - "users/details/*" = All detail operations for users
///
/// Standard Contexts:
/// - {resource} (singular): Entity-level operations (create, delete)
/// - list: Collection views and operations (view, export)
/// - details: Individual resource details (view, manage)
/// - security: Security-related operations (lock, unlock, password)
/// - roles/permissions: Assignment operations
/// - settings: Configuration operations
/// - status: State change operations
///
/// Standard Actions:
/// - view: Read-only access
/// - manage: Full control (edit, update, configure)
/// - create: Create new entities
/// - delete: Remove entities
/// - export: Export data
/// - assign/revoke: Relationship management
/// - lock/unlock, activate/deactivate: State changes
/// </remarks>
public static class Permissions
{
    /// <summary>
    /// Super admin wildcard - grants access to all permissions across all resources.
    /// </summary>
    public const string All = "*";

    /// <summary>
    /// User management permissions.
    /// Controls access to user accounts, profiles, security settings, and role assignments.
    /// </summary>
    public static class Users
    {
        public const string Resource = "users";
        public const string All = $"{Resource}/*";

        // Entity-level operations
        public const string Create = $"{Resource}/user/create";
        public const string Delete = $"{Resource}/user/delete";

        // List operations
        public const string ViewList = $"{Resource}/list/view";
        public const string ExportList = $"{Resource}/list/export";

        // Details operations
        public const string ViewDetails = $"{Resource}/details/view";
        public const string ManageDetails = $"{Resource}/details/manage";

        // Security operations
        public const string ManageSecurity = $"{Resource}/security/manage";
        public const string ChangePassword = $"{Resource}/security/change-password";
        public const string Lock = $"{Resource}/security/lock";
        public const string Unlock = $"{Resource}/security/unlock";

        // Role assignment operations
        public const string ViewRoles = $"{Resource}/roles/view";
        public const string AssignRole = $"{Resource}/roles/assign";
        public const string RevokeRole = $"{Resource}/roles/revoke";
    }

    /// <summary>
    /// Role management permissions.
    /// Controls access to roles, role definitions, and permission assignments.
    /// </summary>
    public static class Roles
    {
        public const string Resource = "roles";
        public const string All = $"{Resource}/*";

        // Entity-level operations
        public const string Create = $"{Resource}/role/create";
        public const string Delete = $"{Resource}/role/delete";

        // List operations
        public const string ViewList = $"{Resource}/list/view";
        public const string ExportList = $"{Resource}/list/export";

        // Details operations
        public const string ViewDetails = $"{Resource}/details/view";
        public const string ManageDetails = $"{Resource}/details/manage";

        // Permission assignment operations
        public const string ViewPermissions = $"{Resource}/permissions/view";
        public const string AssignPermission = $"{Resource}/permissions/assign";
        public const string RevokePermission = $"{Resource}/permissions/revoke";
    }

    /// <summary>
    /// Permission management permissions.
    /// Controls access to the permission system itself (meta-permissions).
    /// </summary>
    public static class PermissionManagement
    {
        public const string Resource = "permissions";
        public const string All = $"{Resource}/*";

        // Entity-level operations
        public const string Create = $"{Resource}/permission/create";
        public const string Delete = $"{Resource}/permission/delete";

        // List operations
        public const string ViewList = $"{Resource}/list/view";
        public const string ExportList = $"{Resource}/list/export";

        // Details operations
        public const string ViewDetails = $"{Resource}/details/view";
        public const string ManageDetails = $"{Resource}/details/manage";
    }

    /// <summary>
    /// Tenant management permissions (for multi-tenancy).
    /// Controls access to tenant creation, configuration, and status management.
    /// </summary>
    public static class Tenants
    {
        public const string Resource = "tenants";
        public const string All = $"{Resource}/*";

        // Entity-level operations
        public const string Create = $"{Resource}/tenant/create";
        public const string Delete = $"{Resource}/tenant/delete";

        // List operations
        public const string ViewList = $"{Resource}/list/view";
        public const string ExportList = $"{Resource}/list/export";

        // Details operations
        public const string ViewDetails = $"{Resource}/details/view";
        public const string ManageDetails = $"{Resource}/details/manage";

        // Settings operations
        public const string ViewSettings = $"{Resource}/settings/view";
        public const string ManageSettings = $"{Resource}/settings/manage";

        // Status operations
        public const string Activate = $"{Resource}/status/activate";
        public const string Deactivate = $"{Resource}/status/deactivate";
    }

    /// <summary>
    /// Authentication and self-service permissions.
    /// Note: These permissions apply to the current user's own resources.
    /// Access to other users' profiles/passwords requires Users.* permissions.
    /// </summary>
    /// <remarks>
    /// Auth permissions are implicit for authenticated users - they don't require
    /// explicit permission grants. They're defined here for completeness and
    /// potential future policy-based restrictions.
    /// </remarks>
    public static class Auth
    {
        public const string Resource = "auth";
        public const string All = $"{Resource}/*";

        // Authentication operations (typically unrestricted)
        public const string Login = $"{Resource}/login";
        public const string Logout = $"{Resource}/logout";

        // Token operations
        public const string RefreshToken = $"{Resource}/token/refresh";
        public const string RevokeToken = $"{Resource}/token/revoke";

        // Profile operations (own profile)
        public const string ViewProfile = $"{Resource}/profile/view";
        public const string ManageProfile = $"{Resource}/profile/manage";

        // Password operations (own password)
        public const string ChangePassword = $"{Resource}/password/change";
    }

    /// <summary>
    /// Project management permissions (App scope).
    /// Controls access to project creation, editing, archiving, and member management.
    /// </summary>
    public static class Projects
    {
        public const string Resource = "projects";
        public const string All = $"{Resource}/*";

        // Entity-level operations
        public const string Create = $"{Resource}/project/create";
        public const string Delete = $"{Resource}/project/delete";

        // List operations
        public const string ViewList = $"{Resource}/list/view";
        public const string ExportList = $"{Resource}/list/export";

        // Details operations
        public const string ViewDetails = $"{Resource}/details/view";
        public const string ManageDetails = $"{Resource}/details/manage";

        // Settings operations
        public const string ViewSettings = $"{Resource}/settings/view";
        public const string ManageSettings = $"{Resource}/settings/manage";

        // Member management
        public const string ViewMembers = $"{Resource}/members/view";
        public const string AddMember = $"{Resource}/members/add";
        public const string RemoveMember = $"{Resource}/members/remove";

        // Status operations
        public const string Archive = $"{Resource}/status/archive";
        public const string Restore = $"{Resource}/status/restore";
    }

    /// <summary>
    /// Task management permissions (App scope).
    /// Controls access to task creation, editing, assignment, and status changes.
    /// </summary>
    public static class Tasks
    {
        public const string Resource = "tasks";
        public const string All = $"{Resource}/*";

        // Entity-level operations
        public const string Create = $"{Resource}/task/create";
        public const string Delete = $"{Resource}/task/delete";

        // List operations
        public const string ViewList = $"{Resource}/list/view";
        public const string ExportList = $"{Resource}/list/export";

        // Details operations
        public const string ViewDetails = $"{Resource}/details/view";
        public const string ManageDetails = $"{Resource}/details/manage";

        // Assignment operations
        public const string Assign = $"{Resource}/assign";
        public const string Unassign = $"{Resource}/unassign";

        // Status operations
        public const string Complete = $"{Resource}/status/complete";
        public const string Reopen = $"{Resource}/status/reopen";

        // Comment operations
        public const string ViewComments = $"{Resource}/comments/view";
        public const string AddComment = $"{Resource}/comments/add";
        public const string DeleteComment = $"{Resource}/comments/delete";
    }
}
