using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyTodos.BuildingBlocks.Application.Contracts.DomainEvents;
using MyTodos.BuildingBlocks.Application.Contracts.Messaging;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Infrastructure.Messaging.Configuration;
using MyTodos.BuildingBlocks.Infrastructure.Messaging.DomainEvents;
using MyTodos.BuildingBlocks.Infrastructure.Messaging.Outbox;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Repositories;

namespace MyTodos.BuildingBlocks.Infrastructure;

/// <summary>
/// Dependency injection setup for BuildingBlocks infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers BuildingBlocks infrastructure services including domain event dispatcher,
    /// outbox pattern, and RabbitMQ messaging.
    /// </summary>
    public static IServiceCollection AddBuildingBlocksInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure settings
        var rabbitMqSection = configuration.GetSection("RabbitMQ");
        services.Configure<RabbitMqSettings>(rabbitMqSection);

        var outboxSection = configuration.GetSection("OutboxProcessor");
        services.Configure<OutboxProcessorSettings>(outboxSection);

        // Register domain event dispatcher
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        // Register messaging services
        services.AddScoped<IRabbitMqPublisher, Messaging.RabbitMqPublisher>();

        // Register persistence services
        services.AddScoped<IOutboxRepository, OutboxRepository>();

        // Register background services
        services.AddHostedService<OutboxProcessorService>();

        return services;
    }
}
