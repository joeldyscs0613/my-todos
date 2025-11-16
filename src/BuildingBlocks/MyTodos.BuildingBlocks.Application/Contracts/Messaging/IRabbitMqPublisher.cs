namespace MyTodos.BuildingBlocks.Application.Contracts.Messaging;

/// <summary>
/// Publisher for sending messages to RabbitMQ.
/// </summary>
public interface IRabbitMqPublisher
{
    /// <summary>
    /// Publishes a message to RabbitMQ exchange.
    /// </summary>
    /// <param name="eventType">The type name of the event (used for routing)</param>
    /// <param name="message">The JSON serialized message content</param>
    /// <param name="ct">Cancellation token</param>
    Task PublishAsync(string eventType, string message, CancellationToken ct = default);
}