using MyTodos.SharedKernel.Helpers;

namespace MyTodos.SharedKernel.Abstractions;

/// <summary>
/// Base record for domain events that represent significant occurrences in the domain.
/// Domain events are immutable and compared by value.
/// </summary>
public abstract record DomainEvent
{
    /// <summary>
    /// Gets the type of the event (e.g., "TaskCreated", "TaskCompleted").
    /// </summary>
    public string EventType { get; init; }

    /// <summary>
    /// Gets the type of the aggregate that raised this event (e.g., "Task", "Project").
    /// </summary>
    public string AggregateType { get; init; }

    /// <summary>
    /// Gets the identifier of the aggregate instance that raised this event.
    /// </summary>
    public string AggregateId { get; init; }

    /// <summary>
    /// Gets the UTC timestamp when this event occurred.
    /// </summary>
    public DateTimeOffset OccurredOn { get; init; }

    /// <summary>
    /// Initializes a new instance of the domain event with the specified details.
    /// </summary>
    /// <param name="eventType">The type of the event.</param>
    /// <param name="aggregateType">The type of the aggregate that raised this event.</param>
    /// <param name="aggregateId">The identifier of the aggregate instance that raised this event.</param>
    /// <exception cref="ArgumentException">Thrown when any parameter is null, empty, or whitespace.</exception>
    protected DomainEvent(
        string eventType,
        string aggregateType,
        string aggregateId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
        ArgumentException.ThrowIfNullOrWhiteSpace(aggregateType);
        ArgumentException.ThrowIfNullOrWhiteSpace(aggregateId);

        EventType = eventType;
        AggregateType = aggregateType;
        AggregateId = aggregateId;
        OccurredOn = DateTimeOffsetHelper.UtcNow;
    }

    /// <summary>
    /// Parameterless constructor for deserialization only (EF Core, JSON serializers).
    /// DO NOT USE in domain code.
    /// </summary>
    [Obsolete("Only for deserialization. Use parameterized constructor.", error: true)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    protected DomainEvent() { }
}
