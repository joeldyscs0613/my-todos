using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Infrastructure;
using MyTodos.Services.NotificationService.Application.Common.Contracts;
using MyTodos.Services.NotificationService.Application.Notifications.Contracts;
using MyTodos.Services.NotificationService.Infrastructure.Email;
using MyTodos.Services.NotificationService.Infrastructure.Messaging;
using MyTodos.Services.NotificationService.Infrastructure.Persistence;
using MyTodos.Services.NotificationService.Infrastructure.Persistence.Repositories;

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
        services.AddScoped<INotificationWriteRepository, NotificationWriteRepository>();
        services.AddScoped<INotificationReadRepository, NotificationReadRepository>();

        // 5. DbContext for BuildingBlocks (important!)
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<NotificationDbContext>());

        // 6. Seeding
        services.AddScoped<DatabaseSeederService>();

        // 7. Email Service
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.AddScoped<IEmailService, SmtpEmailService>();

        // 8. RabbitMQ Consumer
        services.Configure<RabbitMqConsumerSettings>(
            configuration.GetSection(RabbitMqConsumerSettings.SectionName));
        services.AddHostedService<RabbitMqConsumerService>();

        // 9. BuildingBlocks infrastructure (RabbitMQ settings configured here)
        services.AddBuildingBlocksInfrastructure(configuration);

        return services;
    }
}
