using MyTodos.BuildingBlocks.Application.Abstractions.IntegrationEvents;

namespace MyTodos.BuildingBlocks.Application.Contracts.IntegrationEvents;

/// <summary>
/// Defines a handler for integration events.
/// Integration event handlers consume events from message brokers (e.g., RabbitMQ).
/// </summary>
/// <typeparam name="TIntegrationEvent">The type of integration event to handle</typeparam>
public interface IIntegrationEventHandler<in TIntegrationEvent>
    where TIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// Handles the integration event.
    /// </summary>
    /// <param name="integrationEvent">The integration event</param>
    /// <param name="ct">Cancellation token</param>
    Task HandleAsync(TIntegrationEvent integrationEvent, CancellationToken ct = default);
}