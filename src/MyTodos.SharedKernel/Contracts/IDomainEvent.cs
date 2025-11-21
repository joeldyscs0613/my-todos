using MediatR;

namespace MyTodos.SharedKernel.Contracts;

/// <summary>
/// Marker interface for domain events that can be published via MediatR.
/// </summary>
public interface IDomainEvent : INotification
{
}