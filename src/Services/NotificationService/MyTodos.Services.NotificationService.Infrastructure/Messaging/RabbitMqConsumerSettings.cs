namespace MyTodos.Services.NotificationService.Infrastructure.Messaging;

/// <summary>
/// Configuration settings for RabbitMQ consumer.
/// </summary>
public sealed class RabbitMqConsumerSettings
{
    public const string SectionName = "RabbitMqConsumer";

    /// <summary>
    /// Queue name to consume messages from.
    /// </summary>
    public string QueueName { get; init; } = "notification-service-queue";

    /// <summary>
    /// Exchange name to bind the queue to.
    /// </summary>
    public string Exchange { get; init; } = "mytodos.events";

    /// <summary>
    /// Routing key patterns to bind. Empty means bind to all messages.
    /// </summary>
    public string RoutingKey { get; init; } = "#"; // Subscribe to all routing keys

    /// <summary>
    /// Maximum number of retry attempts for failed messages.
    /// </summary>
    public int MaxRetries { get; init; } = 3;

    /// <summary>
    /// Initial retry delay in milliseconds.
    /// </summary>
    public int InitialRetryDelayMs { get; init; } = 1000;

    /// <summary>
    /// Retry delay multiplier for exponential backoff.
    /// </summary>
    public double RetryBackoffMultiplier { get; init; } = 2.0;

    /// <summary>
    /// Number of messages to prefetch from the queue.
    /// </summary>
    public ushort PrefetchCount { get; init; } = 10;
}
