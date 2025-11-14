using MediatR;
using MyTodos.BuildingBlocks.Application.Contracts;
using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.BuildingBlocks.Infrastructure.Messaging.DomainEvents;

/// <summary>
/// Dispatches domain events to registered handlers via MediatR's notification pipeline.
/// Events are published after successful database save to maintain consistency.
/// </summary>
public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IPublisher _publisher;

    public DomainEventDispatcher(IPublisher publisher)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    }

    /// <summary>
    /// Dispatches domain events sequentially to maintain event order.
    /// Each event is published as INotification through MediatR pipeline.
    /// </summary>
    public async Task DispatchAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvents);

        var eventsList = domainEvents.ToList();

        if (!eventsList.Any())
        {
            return;
        }

        // Dispatch events sequentially to maintain order
        // Domain event handlers may depend on execution order
        foreach (var domainEvent in eventsList)
        {
            await _publisher.Publish(domainEvent, ct);
        }
    }
}
