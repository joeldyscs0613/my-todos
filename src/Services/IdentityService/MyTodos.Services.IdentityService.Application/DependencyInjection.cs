using Microsoft.Extensions.DependencyInjection;
using MyTodos.BuildingBlocks.Application;

namespace MyTodos.Services.IdentityService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityServiceApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // BuildingBlocks handles MediatR, validators, behaviors, and domain event handlers
        services.AddBuildingBlocksApplication(assembly);

        // Future: Register IdentityService-specific application services here

        return services;
    }
}
