using Microsoft.Extensions.DependencyInjection;
using MyTodos.BuildingBlocks.Application;

namespace MyTodos.Services.NotificationService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationServiceApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // BuildingBlocks handles MediatR, validators, behaviors
        services.AddBuildingBlocksApplication(assembly);

        // Register service-specific services if needed

        return services;
    }
}
