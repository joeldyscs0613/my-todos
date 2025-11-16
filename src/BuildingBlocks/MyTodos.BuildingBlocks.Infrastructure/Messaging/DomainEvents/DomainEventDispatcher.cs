using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyTodos.BuildingBlocks.Application.Contracts;
using MyTodos.BuildingBlocks.Application.Contracts.DomainEvents;
using MyTodos.SharedKernel.Contracts;

namespace MyTodos.BuildingBlocks.Infrastructure.Messaging.DomainEvents;

/// <summary>
/// Dispatches domain events to their registered handlers.
/// Uses the service provider to dynamically resolve handlers at runtime.
/// </summary>
public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(
        IServiceProvider serviceProvider,
        ILogger<DomainEventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken ct = default)
    {
        var eventsList = domainEvents.ToList();

        if (!eventsList.Any())
        {
            return;
        }

        _logger.LogInformation("Dispatching {Count} domain events", eventsList.Count);

        foreach (var domainEvent in eventsList)
        {
            await DispatchEventAsync(domainEvent, ct);
        }

        _logger.LogInformation("Successfully dispatched {Count} domain events", eventsList.Count);
    }

    private async Task DispatchEventAsync(IDomainEvent domainEvent, CancellationToken ct)
    {
        var eventType = domainEvent.GetType();
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);

        _logger.LogDebug("Dispatching domain event: {EventType}", eventType.Name);

        // Get all registered handlers for this event type
        var handlers = _serviceProvider.GetServices(handlerType);

        var handlersList = handlers.ToList();
        if (!handlersList.Any())
        {
            _logger.LogWarning("No handlers registered for domain event: {EventType}", eventType.Name);
            return;
        }

        // Execute each handler
        foreach (var handler in handlersList)
        {
            if (handler == null) continue;

            try
            {
                var handleMethod = handlerType.GetMethod("Handle");
                if (handleMethod != null)
                {
                    var task = (Task?)handleMethod.Invoke(handler, new object[] { domainEvent, ct });
                    if (task != null)
                    {
                        await task;
                    }
                }

                _logger.LogDebug(
                    "Successfully executed handler {HandlerType} for event {EventType}",
                    handler.GetType().Name,
                    eventType.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error executing handler {HandlerType} for event {EventType}",
                    handler.GetType().Name,
                    eventType.Name);

                // Don't rethrow - we want to continue processing other handlers
                // The error is logged above for investigation
            }
        }
    }
}
