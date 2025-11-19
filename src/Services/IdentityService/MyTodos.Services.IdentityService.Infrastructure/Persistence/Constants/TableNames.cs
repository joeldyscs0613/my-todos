namespace MyTodos.Services.IdentityService.Infrastructure.Persistence.Constants;

/// <summary>
/// Defines constant table names for the Identity Service database.
/// Uses singular naming convention for consistency.
/// </summary>
public static class TableNames
{
    // Aggregate Root Tables
    public const string User = nameof(User);
    public const string Role = nameof(Role);
    public const string Permission = nameof(Permission);
    public const string Tenant = nameof(Tenant);

    // Entity Tables (within aggregates or standalone)
    public const string UserRole = nameof(UserRole);
    public const string RolePermission = nameof(RolePermission);
    public const string UserInvitation = nameof(UserInvitation);
}
