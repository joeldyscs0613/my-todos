using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.BuildingBlocks.Infrastructure;
using MyTodos.Services.NotificationService.Infrastructure.Persistence;
using MyTodos.Services.NotificationService.Infrastructure.Seeding;
using MyTodos.Services.NotificationService.Infrastructure.Security;

namespace MyTodos.Services.NotificationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationServiceInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. DbContext
        var connectionString = configuration.GetConnectionString("NotificationServiceDb")
            ?? "Data Source=notificationservice.db";

        services.AddDbContext<NotificationServiceDbContext>(options =>
            options.UseSqlite(connectionString));

        // 2. Security services
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // 3. UnitOfWork
        services.AddScoped<IUnitOfWork, NotificationServiceUnitOfWork>();

        // 4. Repositories
        // Add repository registrations here as they are created
        // Example:
        // services.AddScoped<INotificationReadRepository, NotificationReadRepository>();
        // services.AddScoped<INotificationWriteRepository, NotificationWriteRepository>();

        // 5. DbContext for BuildingBlocks (important!)
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<NotificationServiceDbContext>());

        // 6. Seeding
        services.AddScoped<DatabaseSeederService>();

        // 7. BuildingBlocks infrastructure
        services.AddBuildingBlocksInfrastructure(configuration);

        return services;
    }
}
