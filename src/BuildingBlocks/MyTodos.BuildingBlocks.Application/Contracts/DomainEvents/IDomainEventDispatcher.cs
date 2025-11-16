using MyTodos.SharedKernel.Contracts;

namespace MyTodos.BuildingBlocks.Application.Contracts.DomainEvents;

/// <summary>
/// Dispatcher for domain events.
/// Resolves and invokes all registered handlers for domain events after the aggregate changes are persisted.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatches a collection of domain events to their registered handlers.
    /// </summary>
    /// <param name="domainEvents">The domain events to dispatch</param>
    /// <param name="ct">Cancellation token</param>
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken ct = default);
}
