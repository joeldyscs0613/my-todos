namespace MyTodos.Services.IdentityService.Contracts;

/// <summary>
/// Centralized registry of all permissions with metadata for UI display and documentation.
/// </summary>
public static class PermissionRegistry
{
    private static readonly Dictionary<string, PermissionMetadata> _permissionsById;
    private static readonly IReadOnlyList<PermissionMetadata> _allPermissions;

    static PermissionRegistry()
    {
        _allPermissions = BuildPermissionMetadata();
        _permissionsById = _allPermissions.ToDictionary(p => p.Permission, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets all registered permissions with their metadata.
    /// </summary>
    public static IReadOnlyList<PermissionMetadata> All => _allPermissions;

    /// <summary>
    /// Gets permission metadata by permission string.
    /// </summary>
    /// <param name="permission">The permission string (e.g., "users/details/view")</param>
    /// <returns>The permission metadata, or null if not found</returns>
    public static PermissionMetadata? Get(string permission)
    {
        return _permissionsById.TryGetValue(permission, out var metadata) ? metadata : null;
    }

    /// <summary>
    /// Gets all permissions for a specific resource.
    /// </summary>
    /// <param name="resource">The resource name (e.g., "users")</param>
    /// <returns>Collection of permissions for the resource</returns>
    public static IEnumerable<PermissionMetadata> GetByResource(string resource)
    {
        return _allPermissions.Where(p => p.Resource.Equals(resource, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets all permissions for a specific category.
    /// </summary>
    /// <param name="category">The category name (e.g., "User Management")</param>
    /// <returns>Collection of permissions in the category</returns>
    public static IEnumerable<PermissionMetadata> GetByCategory(string category)
    {
        return _allPermissions.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets all permissions for a specific context within a resource.
    /// </summary>
    /// <param name="resource">The resource name</param>
    /// <param name="context">The context name (e.g., "list", "details")</param>
    /// <returns>Collection of permissions matching the resource and context</returns>
    public static IEnumerable<PermissionMetadata> GetByResourceContext(string resource, string context)
    {
        return _allPermissions.Where(p =>
            p.Resource.Equals(resource, StringComparison.OrdinalIgnoreCase) &&
            p.Context.Equals(context, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets all read-only (view) permissions.
    /// </summary>
    public static IEnumerable<PermissionMetadata> GetReadOnlyPermissions()
    {
        return _allPermissions.Where(p => p.IsReadOnly);
    }

    /// <summary>
    /// Gets all write permissions (manage, create, delete, etc.).
    /// </summary>
    public static IEnumerable<PermissionMetadata> GetWritePermissions()
    {
        return _allPermissions.Where(p => p.IsWritePermission);
    }

    private static List<PermissionMetadata> BuildPermissionMetadata()
    {
        return new List<PermissionMetadata>
        {
            // Global wildcard
            new(Permissions.All, "Super Admin", "Full access to all system features and resources", "System", "*", "*", "*"),

            // ==================== Users ====================
            new(Permissions.Users.All, "All User Permissions", "Complete access to all user management features", "User Management", "users", "*", "*"),

            // Entity-level
            new(Permissions.Users.Create, "Create User", "Create new user accounts in the system", "User Management", "users", "user", "create"),
            new(Permissions.Users.Delete, "Delete User", "Permanently delete user accounts from the system", "User Management", "users", "user", "delete"),

            // List operations
            new(Permissions.Users.ViewList, "View User List", "View and browse the list of all users", "User Management", "users", "list", "view"),
            new(Permissions.Users.ExportList, "Export User List", "Export user list to various formats (CSV, Excel, PDF)", "User Management", "users", "list", "export"),

            // Details operations
            new(Permissions.Users.ViewDetails, "View User Details", "View detailed information about individual users", "User Management", "users", "details", "view"),
            new(Permissions.Users.ManageDetails, "Manage User Details", "Edit and update user profile information and settings", "User Management", "users", "details", "manage"),

            // Security operations
            new(Permissions.Users.ManageSecurity, "Manage User Security", "Manage user security settings including 2FA and session management", "User Management", "users", "security", "manage"),
            new(Permissions.Users.ChangePassword, "Change User Password", "Reset or change passwords for user accounts", "User Management", "users", "security", "change-password"),
            new(Permissions.Users.Lock, "Lock User Account", "Lock user accounts to prevent access", "User Management", "users", "security", "lock"),
            new(Permissions.Users.Unlock, "Unlock User Account", "Unlock previously locked user accounts", "User Management", "users", "security", "unlock"),

            // Role assignment operations
            new(Permissions.Users.ViewRoles, "View User Roles", "View roles assigned to users", "User Management", "users", "roles", "view"),
            new(Permissions.Users.AssignRole, "Assign Role to User", "Assign roles to user accounts", "User Management", "users", "roles", "assign"),
            new(Permissions.Users.RevokeRole, "Revoke Role from User", "Remove roles from user accounts", "User Management", "users", "roles", "revoke"),

            // ==================== Roles ====================
            new(Permissions.Roles.All, "All Role Permissions", "Complete access to all role management features", "Role Management", "roles", "*", "*"),

            // Entity-level
            new(Permissions.Roles.Create, "Create Role", "Create new roles with custom permissions", "Role Management", "roles", "role", "create"),
            new(Permissions.Roles.Delete, "Delete Role", "Permanently delete roles from the system", "Role Management", "roles", "role", "delete"),

            // List operations
            new(Permissions.Roles.ViewList, "View Role List", "View and browse the list of all roles", "Role Management", "roles", "list", "view"),
            new(Permissions.Roles.ExportList, "Export Role List", "Export role list to various formats", "Role Management", "roles", "list", "export"),

            // Details operations
            new(Permissions.Roles.ViewDetails, "View Role Details", "View detailed information about individual roles", "Role Management", "roles", "details", "view"),
            new(Permissions.Roles.ManageDetails, "Manage Role Details", "Edit and update role information and settings", "Role Management", "roles", "details", "manage"),

            // Permission assignment operations
            new(Permissions.Roles.ViewPermissions, "View Role Permissions", "View permissions assigned to roles", "Role Management", "roles", "permissions", "view"),
            new(Permissions.Roles.AssignPermission, "Assign Permission to Role", "Assign permissions to roles", "Role Management", "roles", "permissions", "assign"),
            new(Permissions.Roles.RevokePermission, "Revoke Permission from Role", "Remove permissions from roles", "Role Management", "roles", "permissions", "revoke"),

            // ==================== Permissions ====================
            new(Permissions.PermissionManagement.All, "All Permission Permissions", "Complete access to permission system management", "Permission Management", "permissions", "*", "*"),

            // Entity-level
            new(Permissions.PermissionManagement.Create, "Create Permission", "Create new system permissions", "Permission Management", "permissions", "permission", "create"),
            new(Permissions.PermissionManagement.Delete, "Delete Permission", "Delete system permissions", "Permission Management", "permissions", "permission", "delete"),

            // List operations
            new(Permissions.PermissionManagement.ViewList, "View Permission List", "View and browse the list of all permissions", "Permission Management", "permissions", "list", "view"),
            new(Permissions.PermissionManagement.ExportList, "Export Permission List", "Export permission list to various formats", "Permission Management", "permissions", "list", "export"),

            // Details operations
            new(Permissions.PermissionManagement.ViewDetails, "View Permission Details", "View detailed information about individual permissions", "Permission Management", "permissions", "details", "view"),
            new(Permissions.PermissionManagement.ManageDetails, "Manage Permission Details", "Edit and update permission information", "Permission Management", "permissions", "details", "manage"),

            // ==================== Tenants ====================
            new(Permissions.Tenants.All, "All Tenant Permissions", "Complete access to all tenant management features", "Tenant Management", "tenants", "*", "*"),

            // Entity-level
            new(Permissions.Tenants.Create, "Create Tenant", "Create new tenant organizations", "Tenant Management", "tenants", "tenant", "create"),
            new(Permissions.Tenants.Delete, "Delete Tenant", "Permanently delete tenant organizations", "Tenant Management", "tenants", "tenant", "delete"),

            // List operations
            new(Permissions.Tenants.ViewList, "View Tenant List", "View and browse the list of all tenants", "Tenant Management", "tenants", "list", "view"),
            new(Permissions.Tenants.ExportList, "Export Tenant List", "Export tenant list to various formats", "Tenant Management", "tenants", "list", "export"),

            // Details operations
            new(Permissions.Tenants.ViewDetails, "View Tenant Details", "View detailed information about individual tenants", "Tenant Management", "tenants", "details", "view"),
            new(Permissions.Tenants.ManageDetails, "Manage Tenant Details", "Edit and update tenant information", "Tenant Management", "tenants", "details", "manage"),

            // Settings operations
            new(Permissions.Tenants.ViewSettings, "View Tenant Settings", "View tenant configuration and settings", "Tenant Management", "tenants", "settings", "view"),
            new(Permissions.Tenants.ManageSettings, "Manage Tenant Settings", "Edit and update tenant configuration and settings", "Tenant Management", "tenants", "settings", "manage"),

            // Status operations
            new(Permissions.Tenants.Activate, "Activate Tenant", "Activate tenant organizations to enable access", "Tenant Management", "tenants", "status", "activate"),
            new(Permissions.Tenants.Deactivate, "Deactivate Tenant", "Deactivate tenant organizations to suspend access", "Tenant Management", "tenants", "status", "deactivate"),

            // ==================== Invitations ====================
            new(Permissions.Invitations.All, "All Invitation Permissions", "Complete access to all invitation management features", "Invitation Management", "invitations", "*", "*"),

            // Entity-level
            new(Permissions.Invitations.Create, "Create Invitation", "Send invitations to new users", "Invitation Management", "invitations", "invitation", "create"),

            // List operations
            new(Permissions.Invitations.ViewList, "View Invitation List", "View and browse the list of all invitations", "Invitation Management", "invitations", "list", "view"),
            new(Permissions.Invitations.ExportList, "Export Invitation List", "Export invitation list to various formats", "Invitation Management", "invitations", "list", "export"),

            // Details operations
            new(Permissions.Invitations.ViewDetails, "View Invitation Details", "View detailed information about individual invitations", "Invitation Management", "invitations", "details", "view"),
            new(Permissions.Invitations.Cancel, "Cancel Invitation", "Cancel pending invitations", "Invitation Management", "invitations", "details", "cancel"),

            // Special operations
            new(Permissions.Invitations.Accept, "Accept Invitation", "Accept an invitation to join the system", "Invitation Management", "invitations", "accept", "accept"),

            // ==================== Auth ====================
            new(Permissions.Auth.All, "All Auth Permissions", "Complete access to all authentication features", "Authentication", "auth", "*", "*"),

            // Authentication operations
            new(Permissions.Auth.Login, "Login", "Authenticate and access the system", "Authentication", "auth", "login", "login"),
            new(Permissions.Auth.Logout, "Logout", "End current session and logout", "Authentication", "auth", "logout", "logout"),

            // Token operations
            new(Permissions.Auth.RefreshToken, "Refresh Token", "Refresh authentication tokens", "Authentication", "auth", "token", "refresh"),
            new(Permissions.Auth.RevokeToken, "Revoke Token", "Revoke active authentication tokens", "Authentication", "auth", "token", "revoke"),

            // Profile operations
            new(Permissions.Auth.ViewProfile, "View Own Profile", "View your own user profile", "Authentication", "auth", "profile", "view"),
            new(Permissions.Auth.ManageProfile, "Manage Own Profile", "Edit and update your own user profile", "Authentication", "auth", "profile", "manage"),

            // Password operations
            new(Permissions.Auth.ChangePassword, "Change Own Password", "Change your own password", "Authentication", "auth", "password", "change"),
        };
    }
}
