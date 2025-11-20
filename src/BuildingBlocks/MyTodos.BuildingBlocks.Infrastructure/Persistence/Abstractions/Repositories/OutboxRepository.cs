using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MyTodos.BuildingBlocks.Application.Abstractions.IntegrationEvents;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Infrastructure.Messaging.Outbox;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Repositories;

/// <summary>
/// Repository implementation for managing outbox messages.
/// </summary>
public class OutboxRepository : IOutboxRepository
{
    private readonly DbContext _context;

    public OutboxRepository(DbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(IntegrationEvent integrationEvent, CancellationToken ct = default)
    {
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = integrationEvent.GetType().Name,
            Content = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType()),
            OccurredOn = integrationEvent.OccurredOn,
            ProcessedOn = null,
            Error = null,
            RetryCount = 0
        };

        _context.Set<OutboxMessage>().Add(outboxMessage);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<(Guid Id, string Type, string Content, int RetryCount)>> GetUnprocessedAsync(
        int batchSize,
        CancellationToken ct = default)
    {
        // Fetch unprocessed messages and process them in-memory
        // SQLite doesn't support DateTimeOffset in ORDER BY, so we must do client-side ordering
        var unprocessed = await _context.Set<OutboxMessage>()
            .Where(m => m.ProcessedOn == null)
            .ToListAsync(ct);

        var messages = unprocessed
            // Process oldest messages first to maintain event order
            .OrderBy(m => m.OccurredOn)
            // Limit to batch size to prevent overwhelming the system
            .Take(batchSize)
            // Return only the fields needed for processing
            .Select(m => (m.Id, m.Type, m.Content, m.RetryCount))
            .ToList();

        return messages;
    }

    public async Task MarkAsProcessedAsync(Guid id, CancellationToken ct = default)
    {
        var message = await _context.Set<OutboxMessage>()
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        if (message != null)
        {
            // Record successful processing time
            message.ProcessedOn = DateTimeOffsetHelper.UtcNow;
            // Clear any previous error from failed retry attempts
            message.Error = null;
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task MarkAsFailedAsync(
        Guid id,
        string error,
        int retryCount,
        CancellationToken ct = default)
    {
        var message = await _context.Set<OutboxMessage>()
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        if (message != null)
        {
            // Store error message for debugging
            message.Error = error;
            // Update retry count - message stays unprocessed (ProcessedOn remains null) so it can be retried
            message.RetryCount = retryCount;
            await _context.SaveChangesAsync(ct);
        }
    }
}
