using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyTodos.BuildingBlocks.Application.Contracts.Messaging;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Infrastructure.Messaging.Configuration;

namespace MyTodos.BuildingBlocks.Infrastructure.Messaging.Outbox;

/// <summary>
/// Background service that processes outbox messages and publishes them to RabbitMQ.
/// Implements the Transactional Outbox pattern for reliable event publishing.
/// </summary>
public class OutboxProcessorService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<OutboxProcessorService> _logger;
    private readonly OutboxProcessorSettings _settings;

    // We inject IServiceScopeFactory instead of IRabbitMqPublisher and IOutboxRepository directly
    // because this service is a singleton (runs for the app's lifetime) but those services are scoped
    // (they depend on DbContext which must be disposed after each operation to prevent memory leaks).
    // This is the recommended pattern from Microsoft for background services that need scoped dependencies.
    public OutboxProcessorService(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<OutboxProcessorSettings> settings,
        ILogger<OutboxProcessorService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation(
            "Outbox Processor started - Interval: {Interval}s, BatchSize: {BatchSize}, MaxRetries: {MaxRetries}",
            _settings.IntervalSeconds,
            _settings.BatchSize,
            _settings.MaxRetries);

        // Wait a bit for the application to fully start
        await Task.Delay(TimeSpan.FromSeconds(10), ct);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in outbox processor loop");
            }

            await Task.Delay(TimeSpan.FromSeconds(_settings.IntervalSeconds), ct);
        }

        _logger.LogInformation("Outbox Processor stopped");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken ct)
    {
        // Create a new scope for this batch of messages.
        // This gives us fresh instances of scoped services (DbContext, repositories, publisher)
        // that will be properly disposed when the scope ends, preventing memory leaks.
        using var scope = _serviceScopeFactory.CreateScope();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var publisher = scope.ServiceProvider.GetRequiredService<IRabbitMqPublisher>();

        // Fetch the next batch of unprocessed messages, ordered by when they occurred
        var messages = await outboxRepository.GetUnprocessedAsync(_settings.BatchSize, ct);
        var messagesList = messages.ToList();

        if (!messagesList.Any())
        {
            return;
        }

        _logger.LogInformation("Processing {Count} outbox messages", messagesList.Count);

        // Process each message individually - success or failure doesn't affect other messages in batch
        foreach (var (id, type, content, currentRetryCount) in messagesList)
        {
            try
            {
                await publisher.PublishAsync(type, content, ct);
                await outboxRepository.MarkAsProcessedAsync(id, ct);

                _logger.LogDebug("Successfully processed outbox message {Id} of type {Type}", id, type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process outbox message {Id} of type {Type}", id, type);

                // Calculate new retry count for this attempt
                var newRetryCount = currentRetryCount + 1;

                // Check if we should retry or give up
                if (newRetryCount <= _settings.MaxRetries)
                {
                    // Still have retries left - mark as failed so it can be retried later
                    await outboxRepository.MarkAsFailedAsync(
                        id,
                        ex.Message,
                        newRetryCount,
                        ct);

                    _logger.LogWarning(
                        "Marked outbox message {Id} as failed (retry {RetryCount}/{MaxRetries})",
                        id,
                        newRetryCount,
                        _settings.MaxRetries);
                }
                else
                {
                    // Exceeded max retries - mark as permanently failed, won't be retried
                    await outboxRepository.MarkAsFailedAsync(
                        id,
                        $"Max retries exceeded: {ex.Message}",
                        newRetryCount,
                        ct);

                    _logger.LogError(
                        "Outbox message {Id} failed permanently after {RetryCount} retries",
                        id,
                        newRetryCount);
                }
            }
        }

        _logger.LogInformation("Completed processing {Count} outbox messages", messagesList.Count);
    }
}
