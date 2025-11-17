namespace MyTodos.Services.IdentityService.Contracts;

/// <summary>
/// Metadata describing a permission for UI display and documentation purposes.
/// </summary>
/// <param name="Permission">The full permission string (e.g., "users/details/view")</param>
/// <param name="DisplayName">Human-friendly display name (e.g., "View User Details")</param>
/// <param name="Description">Detailed description of what this permission grants</param>
/// <param name="Category">Grouping category for UI organization (e.g., "User Management")</param>
/// <param name="Resource">The resource part of the permission (e.g., "users")</param>
/// <param name="Context">The context part of the permission (e.g., "details")</param>
/// <param name="Action">The action part of the permission (e.g., "view")</param>
public record PermissionMetadata(
    string Permission,
    string DisplayName,
    string Description,
    string Category,
    string Resource,
    string Context,
    string Action)
{
    /// <summary>
    /// Gets a value indicating whether this is a wildcard permission.
    /// </summary>
    public bool IsWildcard => Permission.Contains('*');

    /// <summary>
    /// Gets the depth level of the permission (1 for global, 2 for resource, 3 for full hierarchy).
    /// </summary>
    public int Level => Permission == "*" ? 1 : Permission.Count(c => c == '/') + 1;

    /// <summary>
    /// Gets a value indicating whether this permission grants access to a list/collection.
    /// </summary>
    public bool IsListPermission => Context.Equals("list", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether this permission grants access to details/individual resources.
    /// </summary>
    public bool IsDetailsPermission => Context.Equals("details", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether this is a create or delete operation (entity lifecycle).
    /// </summary>
    public bool IsEntityLifecyclePermission =>
        Action.Equals("create", StringComparison.OrdinalIgnoreCase) ||
        Action.Equals("delete", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether this is a read-only permission.
    /// </summary>
    public bool IsReadOnly => Action.Equals("view", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether this is a write permission.
    /// </summary>
    public bool IsWritePermission =>
        Action.Equals("manage", StringComparison.OrdinalIgnoreCase) ||
        Action.Equals("create", StringComparison.OrdinalIgnoreCase) ||
        Action.Equals("delete", StringComparison.OrdinalIgnoreCase) ||
        Action.Equals("assign", StringComparison.OrdinalIgnoreCase) ||
        Action.Equals("revoke", StringComparison.OrdinalIgnoreCase);
}
