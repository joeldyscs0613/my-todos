using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.IdentityService.Contracts;
using MyTodos.Services.IdentityService.Domain.PermissionAggregate;
using MyTodos.Services.IdentityService.Domain.RoleAggregate;
using MyTodos.Services.IdentityService.Domain.RoleAggregate.Enums;
using MyTodos.Services.IdentityService.Domain.TenantAggregate;
using MyTodos.Services.IdentityService.Domain.TenantAggregate.Enums;
using MyTodos.Services.IdentityService.Domain.UserAggregate;
using MyTodos.Services.IdentityService.Infrastructure.Persistence;

namespace MyTodos.Services.IdentityService.Infrastructure.Seeding;

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

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityServiceDbContext>();
        var passwordHashingService = scope.ServiceProvider.GetRequiredService<IPasswordHashingService>();

        try
        {
            // Ensure database is created and migrations are applied
            await dbContext.Database.MigrateAsync(cancellationToken);

            // Check if already seeded
            if (await dbContext.Users.AnyAsync(cancellationToken))
            {
                _logger.LogInformation("Database already seeded. Skipping initial seed.");
                return;
            }

            _logger.LogInformation("Starting database seeding...");

            // 1. Create System Tenant (for global admins)
            var systemTenantResult = Tenant.Create(
                "System",
                TenantPlan.Enterprise,
                maxUsers: 100
            );
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
            var (globalAdminRole, tenantAdminRole, tenantUserRole) = CreateRoles();
            globalAdminRole.SetCreatedInfo("system");
            tenantAdminRole.SetCreatedInfo("system");
            tenantUserRole.SetCreatedInfo("system");

            // 4. Assign Permissions to Roles (creates RolePermission entities)
            AssignPermissionsToRoles(globalAdminRole, tenantAdminRole, tenantUserRole, permissions);

            // Set audit properties for RolePermissions
            foreach (var rolePermission in globalAdminRole.RolePermissions)
            {
                rolePermission.SetCreatedInfo("system");
            }
            foreach (var rolePermission in tenantAdminRole.RolePermissions)
            {
                rolePermission.SetCreatedInfo("system");
            }
            foreach (var rolePermission in tenantUserRole.RolePermissions)
            {
                rolePermission.SetCreatedInfo("system");
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
            adminUser.AssignGlobalRole(globalAdminRole.Id);

            // Set audit properties for UserRole
            foreach (var userRole in adminUser.UserRoles)
            {
                userRole.SetCreatedInfo("system");
            }

            // 7. Add all entities to DbContext
            await dbContext.Tenants.AddAsync(systemTenant, cancellationToken);
            await dbContext.Permissions.AddRangeAsync(permissions, cancellationToken);
            await dbContext.Roles.AddRangeAsync(new[] { globalAdminRole, tenantAdminRole, tenantUserRole }, cancellationToken);
            await dbContext.Users.AddAsync(adminUser, cancellationToken);

            // 8. Save everything in a single transaction
            await dbContext.SaveChangesAsync(cancellationToken);

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
                throw new InvalidOperationException($"Failed to create permission '{metadata.Permission}': {permissionResult.FirstError.Description}");
            }
            permissions.Add(permissionResult.Value!);
        }

        return permissions;
    }

    private static (Role GlobalAdmin, Role TenantAdmin, Role TenantUser) CreateRoles()
    {
        var globalAdminRoleResult = Role.Create(
            RoleType.GlobalAdmin,
            "global-admin",
            "Global Administrator",
            AccessScope.Global,
            "System-wide administrator with full access"
        );
        if (globalAdminRoleResult.IsFailure)
        {
            throw new InvalidOperationException($"Failed to create global admin role: {globalAdminRoleResult.FirstError.Description}");
        }
        var globalAdminRole = globalAdminRoleResult.Value!;

        var tenantAdminRoleResult = Role.Create(
            RoleType.TenantAdmin,
            "tenant-admin",
            "Tenant Administrator",
            AccessScope.Tenant,
            "Tenant administrator with management permissions"
        );
        if (tenantAdminRoleResult.IsFailure)
        {
            throw new InvalidOperationException($"Failed to create tenant admin role: {tenantAdminRoleResult.FirstError.Description}");
        }
        var tenantAdminRole = tenantAdminRoleResult.Value!;

        var tenantUserRoleResult = Role.Create(
            RoleType.TenantUser,
            "tenant-user",
            "Tenant User",
            AccessScope.Tenant,
            "Regular tenant user with basic permissions"
        );
        if (tenantUserRoleResult.IsFailure)
        {
            throw new InvalidOperationException($"Failed to create tenant user role: {tenantUserRoleResult.FirstError.Description}");
        }
        var tenantUserRole = tenantUserRoleResult.Value!;

        return (globalAdminRole, tenantAdminRole, tenantUserRole);
    }

    private static void AssignPermissionsToRoles(
        Role globalAdminRole,
        Role tenantAdminRole,
        Role tenantUserRole,
        List<Permission> permissions)
    {
        // Global Admin gets wildcard permission (all access)
        var wildcardPermission = permissions.First(p => p.Code == Permissions.All);
        globalAdminRole.AssignPermission(wildcardPermission.Id);

        // Tenant Admin gets comprehensive management permissions
        var tenantAdminPermissionCodes = new[]
        {
            // User management (full access within tenant)
            Permissions.Users.Create, Permissions.Users.Delete,
            Permissions.Users.ViewList, Permissions.Users.ExportList,
            Permissions.Users.ViewDetails, Permissions.Users.ManageDetails,
            Permissions.Users.ManageSecurity, Permissions.Users.ChangePassword,
            Permissions.Users.Lock, Permissions.Users.Unlock,
            Permissions.Users.ViewRoles, Permissions.Users.AssignRole, Permissions.Users.RevokeRole,

            // Role management (read only)
            Permissions.Roles.ViewList, Permissions.Roles.ViewDetails, Permissions.Roles.ViewPermissions,

            // Permission management (read only)
            Permissions.PermissionManagement.ViewList, Permissions.PermissionManagement.ViewDetails,

            // Tenant management (read/update own tenant only)
            Permissions.Tenants.ViewDetails, Permissions.Tenants.ManageDetails,
            Permissions.Tenants.ViewSettings, Permissions.Tenants.ManageSettings,

            // Invitation management (full access within tenant)
            Permissions.Invitations.Create, Permissions.Invitations.ViewList, Permissions.Invitations.ExportList,
            Permissions.Invitations.ViewDetails, Permissions.Invitations.Cancel,

            // Auth permissions
            Permissions.Auth.ViewProfile, Permissions.Auth.ManageProfile,
            Permissions.Auth.ChangePassword
        };

        foreach (var code in tenantAdminPermissionCodes)
        {
            var permission = permissions.First(p => p.Code == code);
            tenantAdminRole.AssignPermission(permission.Id);
        }

        // Tenant User gets basic read permissions
        var tenantUserPermissionCodes = new[]
        {
            Permissions.Users.ViewDetails,
            Permissions.Roles.ViewDetails,
            Permissions.Auth.ViewProfile,
            Permissions.Auth.ManageProfile,
            Permissions.Auth.ChangePassword
        };

        foreach (var code in tenantUserPermissionCodes)
        {
            var permission = permissions.First(p => p.Code == code);
            tenantUserRole.AssignPermission(permission.Id);
        }
    }
}
