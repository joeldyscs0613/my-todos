namespace MyTodos.Services.IdentityService.Contracts;

/// <summary>
/// Centralized permission constants for the IdentityService.
/// These constants can be imported by other services (TodoService, etc.) to avoid hard-coding permission strings.
/// </summary>
/// <remarks>
/// Permission naming convention: {resource}.{action}
/// Examples: users.create, roles.update, permissions.delete
///
/// Wildcard support:
/// - "*" = Super admin (all permissions)
/// - "users.*" = All user permissions (create, read, update, delete, etc.)
/// </remarks>
public static class Permissions
{
    /// <summary>
    /// Super admin wildcard - grants access to all permissions.
    /// </summary>
    public const string All = "*";

    /// <summary>
    /// User management permissions.
    /// </summary>
    public static class Users
    {
        public const string Prefix = "users";
        public const string All = $"{Prefix}.*";

        public const string Create = $"{Prefix}.create";
        public const string Read = $"{Prefix}.read";
        public const string Update = $"{Prefix}.update";
        public const string Delete = $"{Prefix}.delete";
        public const string List = $"{Prefix}.list";
        public const string ChangePassword = $"{Prefix}.change-password";
        public const string AssignRole = $"{Prefix}.assign-role";
        public const string RevokeRole = $"{Prefix}.revoke-role";
        public const string Lock = $"{Prefix}.lock";
        public const string Unlock = $"{Prefix}.unlock";
    }

    /// <summary>
    /// Role management permissions.
    /// </summary>
    public static class Roles
    {
        public const string Prefix = "roles";
        public const string All = $"{Prefix}.*";

        public const string Create = $"{Prefix}.create";
        public const string Read = $"{Prefix}.read";
        public const string Update = $"{Prefix}.update";
        public const string Delete = $"{Prefix}.delete";
        public const string List = $"{Prefix}.list";
        public const string AssignPermission = $"{Prefix}.assign-permission";
        public const string RevokePermission = $"{Prefix}.revoke-permission";
    }

    /// <summary>
    /// Permission management permissions.
    /// </summary>
    public static class PermissionManagement
    {
        public const string Prefix = "permissions";
        public const string All = $"{Prefix}.*";

        public const string Create = $"{Prefix}.create";
        public const string Read = $"{Prefix}.read";
        public const string Update = $"{Prefix}.update";
        public const string Delete = $"{Prefix}.delete";
        public const string List = $"{Prefix}.list";
    }

    /// <summary>
    /// Tenant management permissions (for multi-tenancy).
    /// </summary>
    public static class Tenants
    {
        public const string Prefix = "tenants";
        public const string All = $"{Prefix}.*";

        public const string Create = $"{Prefix}.create";
        public const string Read = $"{Prefix}.read";
        public const string Update = $"{Prefix}.update";
        public const string Delete = $"{Prefix}.delete";
        public const string List = $"{Prefix}.list";
        public const string Activate = $"{Prefix}.activate";
        public const string Deactivate = $"{Prefix}.deactivate";
    }

    /// <summary>
    /// User invitation permissions.
    /// </summary>
    public static class Invitations
    {
        public const string Prefix = "invitations";
        public const string All = $"{Prefix}.*";

        public const string Create = $"{Prefix}.create";
        public const string Read = $"{Prefix}.read";
        public const string Cancel = $"{Prefix}.cancel";
        public const string List = $"{Prefix}.list";
        public const string Accept = $"{Prefix}.accept";
    }

    /// <summary>
    /// Authentication and session management permissions.
    /// </summary>
    public static class Auth
    {
        public const string Prefix = "auth";
        public const string All = $"{Prefix}.*";

        public const string Login = $"{Prefix}.login";
        public const string Logout = $"{Prefix}.logout";
        public const string RefreshToken = $"{Prefix}.refresh-token";
        public const string RevokeToken = $"{Prefix}.revoke-token";
        public const string ChangeOwnPassword = $"{Prefix}.change-own-password";
        public const string ViewOwnProfile = $"{Prefix}.view-own-profile";
        public const string UpdateOwnProfile = $"{Prefix}.update-own-profile";
    }
}
