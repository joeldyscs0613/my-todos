using MyTodos.SharedKernel.Contracts;

namespace MyTodos.BuildingBlocks.Application.Contracts.DomainEvents;

/// <summary>
/// Defines a handler for a domain event.
/// Domain event handlers run within the same transaction boundary as the aggregate that raised the event.
/// </summary>
/// <typeparam name="TDomainEvent">The type of domain event to handle</typeparam>
public interface IDomainEventHandler<in TDomainEvent> where TDomainEvent : IDomainEvent
{
    /// <summary>
    /// Handles the domain event.
    /// </summary>
    /// <param name="domainEvent">The domain event</param>
    /// <param name="ct">Cancellation token</param>
    Task Handle(TDomainEvent domainEvent, CancellationToken ct = default);
}