using Microsoft.Extensions.DependencyInjection;

namespace MyTodos.Services.IdentityService.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityServiceDomain(this IServiceCollection services)
    {
        // Future: Register domain services here if needed

        return services;
    }
}
