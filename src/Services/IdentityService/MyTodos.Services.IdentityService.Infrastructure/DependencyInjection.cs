using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MyTodos.Services.IdentityService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityServiceInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Future: Register DbContext, repositories, and other infrastructure services

        return services;
    }
}
