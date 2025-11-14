using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.BuildingBlocks.Application.Contracts;

/// <summary>
/// Dispatches domain events to registered handlers via MediatR.
/// Called by UnitOfWork after successful database save.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatches domain events to registered handlers asynchronously.
    /// Events are published via MediatR's INotification pipeline.
    /// </summary>
    /// <param name="domainEvents">Collection of domain events to dispatch.</param>
    /// <param name="ct">Cancellation token.</param>
    Task DispatchAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken ct = default);
}
