using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyTodos.BuildingBlocks.Application.Contracts.DomainEvents;
using MyTodos.BuildingBlocks.Application.Contracts.Messaging;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.BuildingBlocks.Infrastructure.Http.Configuration;
using MyTodos.BuildingBlocks.Infrastructure.Http.Handlers;
using MyTodos.BuildingBlocks.Infrastructure.Http.Resilience.Extensions;
using MyTodos.BuildingBlocks.Infrastructure.Messaging.Configuration;
using MyTodos.BuildingBlocks.Infrastructure.Messaging.DomainEvents;
using MyTodos.BuildingBlocks.Infrastructure.Messaging.Outbox;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Repositories;
using MyTodos.BuildingBlocks.Infrastructure.Security;

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

        // Register security services
        services.AddHttpContextAccessor();
        // Use TryAddScoped so services can override with their own implementation
        services.TryAddScoped<ICurrentUserService, CurrentUserService>();

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

    /// <summary>
    /// Adds BuildingBlocks.Http services to the service collection.
    /// </summary>
    public static IServiceCollection AddBuildingBlocksHttp(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure settings from configuration
        services.Configure<ResilienceSettings>(
            configuration.GetSection("Http:Resilience"));

        services.Configure<CorrelationSettings>(
            configuration.GetSection("Http:Correlation"));

        // Register delegating handlers as transient
        services.AddTransient<CorrelationIdDelegatingHandler>();
        services.AddTransient<LoggingDelegatingHandler>();

        return services;
    }

    /// <summary>
    /// Adds a typed HTTP client with standard configuration and resilience policies.
    /// </summary>
    /// <typeparam name="TClient">The interface type of the HTTP client.</typeparam>
    /// <typeparam name="TImplementation">The implementation type of the HTTP client.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configureClient">Optional action to configure the HttpClient.</param>
    /// <param name="addStandardResilience">Whether to add standard resilience policies. Default is true.</param>
    /// <param name="addCorrelationId">Whether to add correlation ID propagation. Default is true.</param>
    /// <param name="addLogging">Whether to add request/response logging. Default is true.</param>
    /// <returns>The HTTP client builder for further configuration.</returns>
    public static IHttpClientBuilder AddTypedHttpClient<TClient, TImplementation>(
        this IServiceCollection services,
        Action<HttpClient>? configureClient = null,
        bool addStandardResilience = true,
        bool addCorrelationId = true,
        bool addLogging = true)
        where TClient : class
        where TImplementation : class, TClient
    {
        var builder = services.AddHttpClient<TClient, TImplementation>(client =>
        {
            configureClient?.Invoke(client);
        });

        // Add delegating handlers in order of execution
        if (addCorrelationId)
        {
            builder.AddHttpMessageHandler<CorrelationIdDelegatingHandler>();
        }

        if (addLogging)
        {
            builder.AddHttpMessageHandler<LoggingDelegatingHandler>();
        }

        // Add standard resilience handler (retry, circuit breaker, timeout)
        if (addStandardResilience)
        {
            builder.AddStandardResilienceHandler();
        }

        return builder;
    }

    /// <summary>
    /// Adds a typed HTTP client with settings from configuration.
    /// </summary>
    /// <typeparam name="TClient">The interface type of the HTTP client.</typeparam>
    /// <typeparam name="TImplementation">The implementation type of the HTTP client.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration section containing HTTP client settings.</param>
    /// <param name="configurationSection">The configuration section path. Default is "Http".</param>
    /// <param name="addStandardResilience">Whether to add standard resilience policies. Default is true.</param>
    /// <param name="addCorrelationId">Whether to add correlation ID propagation. Default is true.</param>
    /// <param name="addLogging">Whether to add request/response logging. Default is true.</param>
    /// <returns>The HTTP client builder for further configuration.</returns>
    public static IHttpClientBuilder AddTypedHttpClient<TClient, TImplementation>(
        this IServiceCollection services,
        IConfiguration configuration,
        string configurationSection = "Http",
        bool addStandardResilience = true,
        bool addCorrelationId = true,
        bool addLogging = true)
        where TClient : class
        where TImplementation : class, TClient
    {
        // Configure settings from configuration
        services.Configure<HttpClientSettings>(
            configuration.GetSection(configurationSection));

        var settings = configuration.GetSection(configurationSection).Get<HttpClientSettings>();

        var builder = services.AddHttpClient<TClient, TImplementation>(client =>
        {
            if (settings != null)
            {
                if (!string.IsNullOrWhiteSpace(settings.BaseAddress))
                {
                    client.BaseAddress = new Uri(settings.BaseAddress);
                }

                client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
            }
        });

        // Add delegating handlers
        if (addCorrelationId)
        {
            builder.AddHttpMessageHandler<CorrelationIdDelegatingHandler>();
        }

        if (addLogging)
        {
            builder.AddHttpMessageHandler<LoggingDelegatingHandler>();
        }

        // Add resilience policies
        if (addStandardResilience && settings?.UseResilience == true)
        {
            builder.AddStandardResilienceHandler();
        }

        return builder;
    }

    /// <summary>
    /// Adds a named HTTP client with standard configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="name">The logical name of the HTTP client.</param>
    /// <param name="configureClient">Optional action to configure the HttpClient.</param>
    /// <param name="addStandardResilience">Whether to add standard resilience policies. Default is true.</param>
    /// <param name="addCorrelationId">Whether to add correlation ID propagation. Default is true.</param>
    /// <param name="addLogging">Whether to add request/response logging. Default is true.</param>
    /// <returns>The HTTP client builder for further configuration.</returns>
    public static IHttpClientBuilder AddNamedHttpClient(
        this IServiceCollection services,
        string name,
        Action<HttpClient>? configureClient = null,
        bool addStandardResilience = true,
        bool addCorrelationId = true,
        bool addLogging = true)
    {
        var builder = services.AddHttpClient(name, client =>
        {
            configureClient?.Invoke(client);
        });

        if (addCorrelationId)
        {
            builder.AddHttpMessageHandler<CorrelationIdDelegatingHandler>();
        }

        if (addLogging)
        {
            builder.AddHttpMessageHandler<LoggingDelegatingHandler>();
        }

        if (addStandardResilience)
        {
            builder.AddStandardResilienceHandler();
        }

        return builder;
    }
}
