using Microsoft.AspNetCore.Http;
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
using MyTodos.Services.IdentityService.Infrastructure.RoleAggregate.Repositories;
using MyTodos.Services.IdentityService.Infrastructure.Security;
using MyTodos.Services.IdentityService.Infrastructure.Seeding;
using MyTodos.Services.IdentityService.Infrastructure.TenantAggregate.Repositories;
using MyTodos.Services.IdentityService.Infrastructure.UserAggregate.Repositories;

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

        // Register repositories
        RegisterRepositories(services);

        // Register security services
        RegisterSecurityServices(services, configuration);

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

        // Tenant repositories
        services.AddScoped<ITenantPagedListReadRepository, TenantPagedListReadRepository>();
        services.AddScoped<ITenantWriteRepository, TenantWriteRepository>();

        // Permission repositories
        services.AddScoped<IPermissionReadRepository, PermissionReadRepository>();
        services.AddScoped<IPermissionWriteRepository, PermissionWriteRepository>();
    }

    private static void RegisterSecurityServices(IServiceCollection services, IConfiguration configuration)
    {
        // Configure JWT settings
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        // Register HttpContextAccessor (required for user context)
        services.AddHttpContextAccessor();

        // Register security services
        services.AddScoped<IPasswordHashingService, PasswordHashingService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ISecurityService, SecurityService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ITenantService, TenantService>();
    }
}
