using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.BuildingBlocks.Infrastructure;
using MyTodos.Services.IdentityService.Application.Permissions.Contracts;
using MyTodos.Services.IdentityService.Application.Roles.Contracts;
using MyTodos.Services.IdentityService.Application.Tenants.Contracts;
using MyTodos.Services.IdentityService.Application.Users.Contracts;
using MyTodos.Services.IdentityService.Infrastructure.Persistence;
using MyTodos.Services.IdentityService.Infrastructure.Persistence.Permissions.Repositories;
using MyTodos.Services.IdentityService.Infrastructure.Persistence.Roles.Repositories;
using MyTodos.Services.IdentityService.Infrastructure.Persistence.Tenants.Repositories;
using MyTodos.Services.IdentityService.Infrastructure.Persistence.Users.Repositories;
using MyTodos.Services.IdentityService.Infrastructure.Security;

namespace MyTodos.Services.IdentityService.Infrastructure;

/// <summary>
/// Dependency injection configuration for IdentityService Infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers infrastructure services including DbContext, repositories, and domain event dispatcher.
    /// </summary>
    public static IServiceCollection AddIdentityServiceInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext
        var connectionString = configuration.GetConnectionString("IdentityServiceDb")
            ?? throw new InvalidOperationException("Connection string 'IdentityServiceDb' not found.");

        services.AddDbContext<IdentityServiceDbContext>(options =>
            options.UseSqlite(connectionString, sqliteOptions =>
            {
                sqliteOptions.MigrationsAssembly(typeof(IdentityServiceDbContext).Assembly.FullName);
            }));

        // Register UnitOfWork (uses the specific DbContext)
        services.AddScoped<MyTodos.BuildingBlocks.Application.Contracts.Persistence.IUnitOfWork, MyTodos.Services.IdentityService.Infrastructure.Persistence.IdentityServiceUnitOfWork>();

        // Register security services (must be before repositories as they depend on ICurrentUserService)
        RegisterSecurityServices(services, configuration);

        // Register repositories (depends on ICurrentUserService)
        RegisterRepositories(services);

        // Register database seeder
        services.AddHostedService<DatabaseSeederService>();

        // Register DbContext for BuildingBlocks infrastructure services
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<IdentityServiceDbContext>());

        // Register BuildingBlocks infrastructure (domain event dispatcher, etc.)
        services.AddBuildingBlocksInfrastructure(configuration);

        return services;
    }

    private static void RegisterRepositories(IServiceCollection services)
    {
        // User repositories
        services.AddScoped<IUserPagedListReadRepository, UserPagedListReadRepository>();
        services.AddScoped<IUserWriteRepository, UserWriteRepository>();

        // Role repositories
        services.AddScoped<IRoleReadRepository, RoleReadRepository>();
        services.AddScoped<IRoleWriteRepository, RoleWriteRepository>();
        services.AddScoped<IRolePagedListReadRepository, RolePagedListReadRepository>();

        // Tenant repositories
        services.AddScoped<ITenantPagedListReadRepository, TenantPagedListReadRepository>();
        services.AddScoped<ITenantWriteRepository, TenantWriteRepository>();

        // Permission repositories
        services.AddScoped<IPermissionReadRepository, PermissionReadRepository>();
        services.AddScoped<IPermissionWriteRepository, PermissionWriteRepository>();
        services.AddScoped<IPermissionPagedListReadRepository, PermissionPagedListReadRepository>();
    }

    private static void RegisterSecurityServices(IServiceCollection services, IConfiguration configuration)
    {
        // Configure JWT settings
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        // CurrentUserService and HttpContextAccessor provided by BuildingBlocks

        // Register IdentityService-specific security services
        services.AddScoped<IPasswordHashingService, PasswordHashingService>();
        services.AddScoped<ISecurityService, SecurityService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ITenantService, TenantService>();
    }
}
