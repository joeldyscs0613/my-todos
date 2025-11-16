using MyTodos.BuildingBlocks.Application.Abstractions.IntegrationEvents;

namespace MyTodos.BuildingBlocks.Application.Contracts.Persistence;

/// <summary>
/// Repository for managing outbox messages (integration events pending delivery).
/// Implements the Transactional Outbox pattern for reliable event publishing.
/// </summary>
public interface IOutboxRepository
{
    /// <summary>
    /// Adds an integration event to the outbox for later processing.
    /// </summary>
    /// <param name="integrationEvent">The integration event to store</param>
    /// <param name="ct">Cancellation token</param>
    Task AddAsync(IntegrationEvent integrationEvent, CancellationToken ct = default);

    /// <summary>
    /// Retrieves unprocessed outbox messages for publishing.
    /// </summary>
    /// <param name="batchSize">Maximum number of messages to retrieve</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Collection of unprocessed outbox messages with ID, Type, Content, and RetryCount</returns>
    Task<IEnumerable<(Guid Id, string Type, string Content, int RetryCount)>> GetUnprocessedAsync(
        int batchSize,
        CancellationToken ct = default);

    /// <summary>
    /// Marks an outbox message as successfully processed.
    /// </summary>
    /// <param name="id">The outbox message ID</param>
    /// <param name="ct">Cancellation token</param>
    Task MarkAsProcessedAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Marks an outbox message as failed with error details.
    /// </summary>
    /// <param name="id">The outbox message ID</param>
    /// <param name="error">Error message</param>
    /// <param name="retryCount">Current retry attempt count</param>
    /// <param name="ct">Cancellation token</param>
    Task MarkAsFailedAsync(Guid id, string error, int retryCount, CancellationToken ct = default);
}