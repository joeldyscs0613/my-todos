using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.SharedKernel.Contracts;

/// <summary>
/// Marker interface for aggregate roots that support domain events.
/// Enables UnitOfWork to collect and dispatch domain events without knowing the aggregate's ID type.
/// </summary>
public interface IAggregateRoot
{
    /// <summary>
    /// Gets the collection of domain events raised by this aggregate.
    /// </summary>
    IReadOnlyCollection<DomainEvent> DomainEvents { get; }

    /// <summary>
    /// Clears all domain events from this aggregate.
    /// Called by UnitOfWork after dispatching events.
    /// </summary>
    void ClearDomainEvents();
}
