using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyTodos.BuildingBlocks.Application.Constants;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.IdentityService.Contracts;
using MyTodos.Services.IdentityService.Domain.PermissionAggregate;
using MyTodos.Services.IdentityService.Domain.RoleAggregate;
using MyTodos.Services.IdentityService.Domain.RoleAggregate.Enums;
using MyTodos.Services.IdentityService.Domain.TenantAggregate;
using MyTodos.Services.IdentityService.Domain.UserAggregate;

namespace MyTodos.Services.IdentityService.Infrastructure.Persistence;

/// <summary>
/// Seeds initial data (roles, permissions, admin user, system tenant) on application startup.
/// </summary>
public sealed class DatabaseSeederService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseSeederService> _logger;

    public DatabaseSeederService(
        IServiceProvider serviceProvider,
        ILogger<DatabaseSeederService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityServiceDbContext>();
        var passwordHashingService = scope.ServiceProvider.GetRequiredService<IPasswordHashingService>();

        try
        {
            // Ensure database is created and migrations are applied
            await dbContext.Database.MigrateAsync(ct);

            // Check if already seeded
            if (await dbContext.Users.AnyAsync(ct))
            {
                _logger.LogInformation("Database already seeded. Skipping initial seed.");
                return;
            }

            _logger.LogInformation("Starting database seeding...");

            // 1. Create System Tenant (for global admins)
            var systemTenantResult = Tenant.Create("System", isActive: true);
            if (systemTenantResult.IsFailure)
            {
                throw new InvalidOperationException($"Failed to create system tenant: {systemTenantResult.FirstError.Description}");
            }
            var systemTenant = systemTenantResult.Value!;
            systemTenant.SetCreatedInfo("system");

            // 2. Create Permissions
            var permissions = CreatePermissions();
            foreach (var permission in permissions)
            {
                permission.SetCreatedInfo("system");
            }

            // 3. Create Roles
            var roles = CreateRoles();
            foreach (var role in roles)
            {
                role.SetCreatedInfo("system");
            }

            // 4. Assign Permissions to Roles (creates RolePermission entities)
            AssignPermissionsToRoles(roles, permissions);

            // Set audit properties for RolePermissions
            foreach (var role in roles)
            {
                foreach (var rolePermission in role.RolePermissions)
                {
                    rolePermission.SetCreatedInfo("system");
                }
            }

            // 5. Create Global Admin User
            var adminUserResult = User.Create(
                "admin",
                "admin@mytodos.com",
                passwordHashingService.HashPassword("Admin123!"),
                "System",
                "Administrator"
            );
            if (adminUserResult.IsFailure)
            {
                throw new InvalidOperationException($"Failed to create admin user: {adminUserResult.FirstError.Description}");
            }
            var adminUser = adminUserResult.Value!;
            adminUser.SetCreatedInfo("system");

            // 6. Assign Global Admin role to admin user (no tenant - global role, creates UserRole entity)
            var globalAdminRole = roles.First(r => r.Code == WellKnownRoles.GlobalAdmin);
            adminUser.AssignGlobalRole(globalAdminRole.Id);

            // Set audit properties for UserRole
            foreach (var userRole in adminUser.UserRoles)
            {
                userRole.SetCreatedInfo("system");
            }

            // 7. Add all entities to DbContext
            await dbContext.Tenants.AddAsync(systemTenant, ct);
            await dbContext.Permissions.AddRangeAsync(permissions, ct);
            await dbContext.Roles.AddRangeAsync(roles, ct);
            await dbContext.Users.AddAsync(adminUser, ct);

            // 8. Save everything in a single transaction
            await dbContext.SaveChangesAsync(ct);

            _logger.LogInformation("Database seeding completed successfully.");
            _logger.LogInformation("Default admin credentials - Username: admin, Password: Admin123!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private static List<Permission> CreatePermissions()
    {
        // Use PermissionRegistry to get all permissions with metadata
        var permissionMetadataList = PermissionRegistry.All;

        var permissions = new List<Permission>();

        foreach (var metadata in permissionMetadataList)
        {
            var permissionResult = Permission.Create(
                metadata.Permission,
                metadata.DisplayName,
                metadata.Description
            );
            if (permissionResult.IsFailure)
            {
                throw new InvalidOperationException($"Failed to create permission '{metadata.Permission}':" +
                                                    $" {permissionResult.FirstError.Description}");
            }
            permissions.Add(permissionResult.Value!);
        }

        return permissions;
    }

    private static List<Role> CreateRoles()
    {
        var roles = new List<Role>();

        // Global scope
        var globalAdmin = CreateRole(
            WellKnownRoles.GlobalAdmin,
            "Global Admin",
            AccessScope.Global,
            "Full platform access across all tenants");
        roles.Add(globalAdmin);

        // Tenant scope
        var tenantAdmin = CreateRole(
            WellKnownRoles.TenantAdmin,
            "Tenant Admin",
            AccessScope.Tenant,
            "Manages users within their tenant");
        roles.Add(tenantAdmin);

        // App scope
        var appAdmin = CreateRole(
            WellKnownRoles.AppAdmin,
            "App Admin",
            AccessScope.App,
            "Full application access for projects and tasks");
        roles.Add(appAdmin);

        var appContributor = CreateRole(
            WellKnownRoles.AppContributor,
            "App Contributor",
            AccessScope.App,
            "Can create and manage projects and tasks");
        roles.Add(appContributor);

        var appObserver = CreateRole(
            WellKnownRoles.AppObserver,
            "App Observer",
            AccessScope.App,
            "Read-only application access");
        roles.Add(appObserver);

        return roles;
    }

    private static Role CreateRole(string code, string name, AccessScope scope, string? description = null)
    {
        var roleResult = Role.Create(code, name, scope, description);
        if (roleResult.IsFailure)
        {
            throw new InvalidOperationException($"Failed to create role '{code}': {roleResult.FirstError.Description}");
        }
        return roleResult.Value!;
    }

    private static void AssignPermissionsToRoles(List<Role> roles, List<Permission> permissions)
    {
        var globalAdmin = roles.First(r => r.Code == WellKnownRoles.GlobalAdmin);
        var tenantAdmin = roles.First(r => r.Code == WellKnownRoles.TenantAdmin);
        var appAdmin = roles.First(r => r.Code == WellKnownRoles.AppAdmin);
        var appContributor = roles.First(r => r.Code == WellKnownRoles.AppContributor);
        var appObserver = roles.First(r => r.Code == WellKnownRoles.AppObserver);

        // ===== GLOBAL.ADMIN =====
        // Gets wildcard permission (all access)
        var wildcardPermission = permissions.First(p => p.Code == Contracts.Permissions.All);
        globalAdmin.AssignPermission(wildcardPermission.Id);

        // ===== TENANT.ADMIN =====
        // User management within their tenant only - NO tenant management permissions
        var tenantAdminPermissionCodes = new[]
        {
            // User management (full access within tenant)
            Contracts.Permissions.Users.Create,
            Contracts.Permissions.Users.Delete,
            Contracts.Permissions.Users.ViewList,
            Contracts.Permissions.Users.ExportList,
            Contracts.Permissions.Users.ViewDetails,
            Contracts.Permissions.Users.ManageDetails,
            Contracts.Permissions.Users.ManageSecurity,
            Contracts.Permissions.Users.ChangePassword,
            Contracts.Permissions.Users.Lock,
            Contracts.Permissions.Users.Unlock,
            Contracts.Permissions.Users.ViewRoles,
            Contracts.Permissions.Users.AssignRole,
            Contracts.Permissions.Users.RevokeRole,

            // Role management (read only - can view roles to assign them)
            Contracts.Permissions.Roles.ViewList,
            Contracts.Permissions.Roles.ViewDetails,
            Contracts.Permissions.Roles.ViewPermissions,

            // Permission management (read only)
            Contracts.Permissions.PermissionManagement.ViewList,
            Contracts.Permissions.PermissionManagement.ViewDetails,

            // Auth permissions
            Contracts.Permissions.Auth.ViewProfile,
            Contracts.Permissions.Auth.ManageProfile,
            Contracts.Permissions.Auth.ChangePassword
        };

        AssignPermissionsToRole(tenantAdmin, tenantAdminPermissionCodes, permissions);

        // ===== APP.ADMIN =====
        // Full application access (projects and tasks)
        var appAdminPermissionCodes = new[]
        {
            // Projects (full access)
            Contracts.Permissions.Projects.All,

            // Tasks (full access)
            Contracts.Permissions.Tasks.All,

            // Auth permissions
            Contracts.Permissions.Auth.ViewProfile,
            Contracts.Permissions.Auth.ManageProfile,
            Contracts.Permissions.Auth.ChangePassword
        };

        AssignPermissionsToRole(appAdmin, appAdminPermissionCodes, permissions);

        // ===== APP.CONTRIBUTOR =====
        // Regular user - can create and manage projects and tasks
        var appContributorPermissionCodes = new[]
        {
            // Projects (create own, view all, manage own)
            Contracts.Permissions.Projects.ViewList,
            Contracts.Permissions.Projects.ViewDetails,
            Contracts.Permissions.Projects.Create,
            Contracts.Permissions.Projects.ManageDetails,
            Contracts.Permissions.Projects.ViewSettings,
            Contracts.Permissions.Projects.ManageSettings,
            Contracts.Permissions.Projects.ViewMembers,
            Contracts.Permissions.Projects.Archive,
            Contracts.Permissions.Projects.Restore,

            // Tasks (full access)
            Contracts.Permissions.Tasks.Create,
            Contracts.Permissions.Tasks.Delete,
            Contracts.Permissions.Tasks.ViewList,
            Contracts.Permissions.Tasks.ViewDetails,
            Contracts.Permissions.Tasks.ManageDetails,
            Contracts.Permissions.Tasks.Assign,
            Contracts.Permissions.Tasks.Unassign,
            Contracts.Permissions.Tasks.Complete,
            Contracts.Permissions.Tasks.Reopen,
            Contracts.Permissions.Tasks.ViewComments,
            Contracts.Permissions.Tasks.AddComment,
            Contracts.Permissions.Tasks.DeleteComment,

            // Auth permissions
            Contracts.Permissions.Auth.ViewProfile,
            Contracts.Permissions.Auth.ManageProfile,
            Contracts.Permissions.Auth.ChangePassword
        };

        AssignPermissionsToRole(appContributor, appContributorPermissionCodes, permissions);

        // ===== APP.OBSERVER =====
        // Read-only application access
        var appObserverPermissionCodes = new[]
        {
            // Projects (read only)
            Contracts.Permissions.Projects.ViewList,
            Contracts.Permissions.Projects.ViewDetails,
            Contracts.Permissions.Projects.ViewSettings,
            Contracts.Permissions.Projects.ViewMembers,

            // Tasks (read only)
            Contracts.Permissions.Tasks.ViewList,
            Contracts.Permissions.Tasks.ViewDetails,
            Contracts.Permissions.Tasks.ViewComments,

            // Auth permissions
            Contracts.Permissions.Auth.ViewProfile,
            Contracts.Permissions.Auth.ManageProfile,
            Contracts.Permissions.Auth.ChangePassword
        };

        AssignPermissionsToRole(appObserver, appObserverPermissionCodes, permissions);
    }

    private static void AssignPermissionsToRole(Role role, string[] permissionCodes, List<Permission> permissions)
    {
        foreach (var code in permissionCodes)
        {
            var permission = permissions.FirstOrDefault(p => p.Code == code);
            if (permission != null)
            {
                role.AssignPermission(permission.Id);
            }
        }
    }
}
