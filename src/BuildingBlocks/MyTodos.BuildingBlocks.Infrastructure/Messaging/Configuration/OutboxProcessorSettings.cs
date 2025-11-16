namespace MyTodos.BuildingBlocks.Infrastructure.Messaging.Configuration;

/// <summary>
/// Outbox processor background service configuration.
/// </summary>
public class OutboxProcessorSettings
{
    /// <summary>
    /// Interval in seconds between outbox processing runs.
    /// </summary>
    public int IntervalSeconds { get; set; } = 5;

    /// <summary>
    /// Maximum number of messages to process per batch.
    /// </summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Maximum number of retry attempts for failed messages.
    /// </summary>
    public int MaxRetries { get; set; } = 3;
}
