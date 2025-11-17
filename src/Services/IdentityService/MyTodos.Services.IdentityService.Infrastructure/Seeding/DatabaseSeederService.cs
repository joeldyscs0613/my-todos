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
            var systemTenant = Tenant.Create(
                "System",
                TenantPlan.Enterprise,
                maxUsers: 100
            );
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
            var adminUser = User.Create(
                "admin",
                "admin@system.com",
                passwordHashingService.HashPassword("Admin123!"),
                "System",
                "Administrator"
            );
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
        return new List<Permission>
        {
            // Wildcard permission (all access)
            Permission.Create(Permissions.All, "All Permissions", "Global wildcard - access to everything"),

            // User management
            Permission.Create(Permissions.Users.Create, "Create User", "Create new users"),
            Permission.Create(Permissions.Users.Read, "View User Details", "View user details"),
            Permission.Create(Permissions.Users.Update, "Update User", "Update user information"),
            Permission.Create(Permissions.Users.Delete, "Delete User", "Delete users"),
            Permission.Create(Permissions.Users.List, "List Users", "List all users"),
            Permission.Create(Permissions.Users.ChangePassword, "Change User Password", "Change any user's password"),
            Permission.Create(Permissions.Users.AssignRole, "Assign Role to User", "Assign roles to users"),
            Permission.Create(Permissions.Users.RevokeRole, "Revoke Role from User", "Revoke roles from users"),
            Permission.Create(Permissions.Users.Lock, "Lock User", "Lock user accounts"),
            Permission.Create(Permissions.Users.Unlock, "Unlock User", "Unlock user accounts"),

            // Role management
            Permission.Create(Permissions.Roles.Create, "Create Role", "Create new roles"),
            Permission.Create(Permissions.Roles.Read, "View Role Details", "View role details"),
            Permission.Create(Permissions.Roles.Update, "Update Role", "Update role information"),
            Permission.Create(Permissions.Roles.Delete, "Delete Role", "Delete roles"),
            Permission.Create(Permissions.Roles.List, "List Roles", "List all roles"),
            Permission.Create(Permissions.Roles.AssignPermission, "Assign Permission to Role", "Assign permissions to roles"),
            Permission.Create(Permissions.Roles.RevokePermission, "Revoke Permission from Role", "Revoke permissions from roles"),

            // Permission management
            Permission.Create(Permissions.PermissionManagement.Create, "Create Permission", "Create new permissions"),
            Permission.Create(Permissions.PermissionManagement.Read, "View Permission Details", "View permission details"),
            Permission.Create(Permissions.PermissionManagement.Update, "Update Permission", "Update permission information"),
            Permission.Create(Permissions.PermissionManagement.Delete, "Delete Permission", "Delete permissions"),
            Permission.Create(Permissions.PermissionManagement.List, "List Permissions", "List all permissions"),

            // Tenant management
            Permission.Create(Permissions.Tenants.Create, "Create Tenant", "Create new tenants"),
            Permission.Create(Permissions.Tenants.Read, "View Tenant Details", "View tenant details"),
            Permission.Create(Permissions.Tenants.Update, "Update Tenant", "Update tenant settings"),
            Permission.Create(Permissions.Tenants.Delete, "Delete Tenant", "Delete tenants"),
            Permission.Create(Permissions.Tenants.List, "List Tenants", "List all tenants"),
            Permission.Create(Permissions.Tenants.Activate, "Activate Tenant", "Activate tenant accounts"),
            Permission.Create(Permissions.Tenants.Deactivate, "Deactivate Tenant", "Deactivate tenant accounts"),

            // Invitation management
            Permission.Create(Permissions.Invitations.Create, "Create Invitation", "Send user invitations"),
            Permission.Create(Permissions.Invitations.Read, "View Invitation Details", "View invitation details"),
            Permission.Create(Permissions.Invitations.Cancel, "Cancel Invitation", "Cancel pending invitations"),
            Permission.Create(Permissions.Invitations.List, "List Invitations", "List all invitations"),
            Permission.Create(Permissions.Invitations.Accept, "Accept Invitation", "Accept invitations"),

            // Authentication permissions
            Permission.Create(Permissions.Auth.Login, "Login", "Sign in to the system"),
            Permission.Create(Permissions.Auth.Logout, "Logout", "Sign out of the system"),
            Permission.Create(Permissions.Auth.RefreshToken, "Refresh Token", "Refresh authentication tokens"),
            Permission.Create(Permissions.Auth.RevokeToken, "Revoke Token", "Revoke authentication tokens"),
            Permission.Create(Permissions.Auth.ChangeOwnPassword, "Change Own Password", "Change your own password"),
            Permission.Create(Permissions.Auth.ViewOwnProfile, "View Own Profile", "View your own profile"),
            Permission.Create(Permissions.Auth.UpdateOwnProfile, "Update Own Profile", "Update your own profile")
        };
    }

    private static (Role GlobalAdmin, Role TenantAdmin, Role TenantUser) CreateRoles()
    {
        var globalAdminRole = Role.Create(
            RoleType.GlobalAdmin,
            "global-admin",
            "Global Administrator",
            AccessScope.Global,
            "System-wide administrator with full access"
        );

        var tenantAdminRole = Role.Create(
            RoleType.TenantAdmin,
            "tenant-admin",
            "Tenant Administrator",
            AccessScope.Tenant,
            "Tenant administrator with management permissions"
        );

        var tenantUserRole = Role.Create(
            RoleType.TenantUser,
            "tenant-user",
            "Tenant User",
            AccessScope.Tenant,
            "Regular tenant user with basic permissions"
        );

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
            Permissions.Users.Create, Permissions.Users.Read, Permissions.Users.Update,
            Permissions.Users.Delete, Permissions.Users.List, Permissions.Users.AssignRole,
            Permissions.Users.RevokeRole, Permissions.Users.Lock, Permissions.Users.Unlock,

            // Role management (read only)
            Permissions.Roles.Read, Permissions.Roles.List,

            // Permission management (read only)
            Permissions.PermissionManagement.Read, Permissions.PermissionManagement.List,

            // Tenant management (read/update own tenant only)
            Permissions.Tenants.Read, Permissions.Tenants.Update,

            // Invitation management (full access within tenant)
            Permissions.Invitations.Create, Permissions.Invitations.Read,
            Permissions.Invitations.Cancel, Permissions.Invitations.List,

            // Auth permissions
            Permissions.Auth.ViewOwnProfile, Permissions.Auth.UpdateOwnProfile,
            Permissions.Auth.ChangeOwnPassword
        };

        foreach (var code in tenantAdminPermissionCodes)
        {
            var permission = permissions.First(p => p.Code == code);
            tenantAdminRole.AssignPermission(permission.Id);
        }

        // Tenant User gets basic read permissions
        var tenantUserPermissionCodes = new[]
        {
            Permissions.Users.Read,
            Permissions.Roles.Read,
            Permissions.Auth.ViewOwnProfile,
            Permissions.Auth.UpdateOwnProfile,
            Permissions.Auth.ChangeOwnPassword
        };

        foreach (var code in tenantUserPermissionCodes)
        {
            var permission = permissions.First(p => p.Code == code);
            tenantUserRole.AssignPermission(permission.Id);
        }
    }
}
