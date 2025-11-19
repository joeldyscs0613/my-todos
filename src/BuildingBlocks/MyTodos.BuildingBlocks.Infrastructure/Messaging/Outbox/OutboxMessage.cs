namespace MyTodos.BuildingBlocks.Infrastructure.Messaging.Outbox;

/// <summary>
/// Represents an integration event stored in the outbox for reliable delivery.
/// Implements the Transactional Outbox pattern.
/// </summary>
public class OutboxMessage
{
    /// <summary>
    /// Unique identifier for the outbox message
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The type name of the integration event (used for deserialization routing)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// JSON serialized content of the integration event
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the event occurred
    /// </summary>
    public DateTimeOffset OccurredOn { get; set; }

    /// <summary>
    /// Timestamp when the message was successfully processed
    /// </summary>
    public DateTimeOffset? ProcessedOn { get; set; }

    /// <summary>
    /// Error message if processing failed
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Number of times processing has been retried
    /// </summary>
    public int RetryCount { get; set; }
}