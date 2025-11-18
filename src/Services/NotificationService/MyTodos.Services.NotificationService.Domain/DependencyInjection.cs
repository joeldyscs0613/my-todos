using Microsoft.Extensions.DependencyInjection;

namespace MyTodos.Services.NotificationService.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationServiceDomain(this IServiceCollection services)
    {
        // Register domain services here if needed
        return services;
    }
}
