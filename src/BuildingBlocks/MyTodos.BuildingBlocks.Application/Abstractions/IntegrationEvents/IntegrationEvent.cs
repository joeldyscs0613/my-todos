using MyTodos.SharedKernel.Helpers;

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
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffsetHelper.UtcNow;

    /// <summary>
    /// Gets the event type name for message routing.
    /// By default, returns the derived class name.
    /// Override this if you want a custom event name different from the class name.
    /// </summary>
    public virtual string GetEventName() => GetType().Name;
}