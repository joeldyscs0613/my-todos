namespace MyTodos.BuildingBlocks.Application.Abstractions.IntegrationEvents;

/// <summary>
/// Base class for integration events.
/// Integration events are used for communication between bounded contexts (microservices).
/// They should only contain primitive types to ensure serialization compatibility.
/// </summary>
public abstract record IntegrationEvent
{
    /// <summary>
    /// Unique identifier for the event instance
    /// </summary>
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Timestamp when the event occurred
    /// </summary>
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// The type name of the event (used for deserialization routing)
    /// </summary>
    public string EventType { get; init; } = string.Empty;
}