using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Infrastructure;
using MyTodos.Services.NotificationService.Infrastructure.Persistence;

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

        services.AddDbContext<NotificationDbContext>(options =>
            options.UseSqlite(connectionString));

        // 2. CurrentUserService and HttpContextAccessor provided by BuildingBlocks

        // 3. UnitOfWork
        services.AddScoped<IUnitOfWork, NotificationServiceUnitOfWork>();

        // 4. Repositories
        // Add repository registrations here as they are created

        // 5. DbContext for BuildingBlocks (important!)
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<NotificationDbContext>());

        // 6. Seeding
        services.AddScoped<DatabaseSeederService>();

        // 7. BuildingBlocks infrastructure
        services.AddBuildingBlocksInfrastructure(configuration);

        return services;
    }
}
