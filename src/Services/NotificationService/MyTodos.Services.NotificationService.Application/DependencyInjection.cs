using Microsoft.Extensions.DependencyInjection;
using MyTodos.BuildingBlocks.Application;
using MyTodos.BuildingBlocks.Application.Contracts.IntegrationEvents;
using MyTodos.Services.IdentityService.Contracts.IntegrationEvents;
using MyTodos.Services.NotificationService.Application.Users.IntegrationEventHandlers;

namespace MyTodos.Services.NotificationService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationServiceApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // BuildingBlocks handles MediatR, validators, behaviors, and domain event handlers
        services.AddBuildingBlocksApplication(assembly);

        // Register integration event handlers
        services.AddScoped<IIntegrationEventHandler<UserCreatedIntegrationEvent>, UserCreatedIntegrationEventHandler>();

        return services;
    }
}
