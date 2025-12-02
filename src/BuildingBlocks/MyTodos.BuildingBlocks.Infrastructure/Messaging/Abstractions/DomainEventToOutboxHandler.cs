using Microsoft.Extensions.Logging;
using MyTodos.BuildingBlocks.Application.Abstractions.IntegrationEvents;
using MyTodos.BuildingBlocks.Application.Contracts.DomainEvents;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.SharedKernel.Contracts;

namespace MyTodos.BuildingBlocks.Infrastructure.Messaging.Abstractions;

/// <summary>
/// Base handler that converts domain events to integration events and saves them to the outbox.
/// Inherit from this to reduce boilerplate in domain event handlers.
/// </summary>
public abstract class DomainEventToOutboxHandler<TDomainEvent> : IDomainEventHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    private readonly IOutboxRepository _outboxRepository;
    private readonly ILogger _logger;

    protected DomainEventToOutboxHandler(
        IOutboxRepository outboxRepository,
        ILogger logger)
    {
        _outboxRepository = outboxRepository;
        _logger = logger;
    }

    public async Task Handle(TDomainEvent domainEvent, CancellationToken ct = default)
    {
        _logger.LogDebug(
            "Converting domain event {EventType} to integration event",
            typeof(TDomainEvent).Name);

        var integrationEvent = await ToIntegrationEvent(domainEvent, ct);

        await _outboxRepository.AddAsync(integrationEvent, ct);

        _logger.LogDebug(
            "Added integration event {EventType} to outbox",
            integrationEvent.GetEventName());
    }

    /// <summary>
    /// Converts the domain event to an integration event.
    /// Override this to map your domain event to the appropriate integration event.
    /// </summary>
    protected abstract Task<IntegrationEvent> ToIntegrationEvent(
        TDomainEvent domainEvent,
        CancellationToken ct);
}
