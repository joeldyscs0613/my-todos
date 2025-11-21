using MediatR;
using Microsoft.Extensions.Logging;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.Services.IdentityService.Contracts.IntegrationEvents;
using MyTodos.Services.IdentityService.Domain.UserAggregate.DomainEvents;

namespace MyTodos.Services.IdentityService.Application.Users.DomainEventHandlers;

/// <summary>
/// Handles UserCreatedDomainEvent by converting it to an integration event for other services.
/// This follows the pattern: Domain Event (internal) -> Integration Event (cross-service).
/// </summary>
public sealed class UserCreatedDomainEventHandler : INotificationHandler<UserCreatedDomainEvent>
{
    private readonly IOutboxRepository _outboxRepository;
    private readonly ILogger<UserCreatedDomainEventHandler> _logger;

    public UserCreatedDomainEventHandler(
        IOutboxRepository outboxRepository,
        ILogger<UserCreatedDomainEventHandler> logger)
    {
        _outboxRepository = outboxRepository;
        _logger = logger;
    }

    public async Task Handle(UserCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling UserCreatedDomainEvent for user {UserId} - converting to integration event",
            domainEvent.UserId);

        // Convert domain event to integration event
        var integrationEvent = new UserCreatedIntegrationEvent
        {
            UserId = domainEvent.UserId,
            Username = domainEvent.Username,
            Email = domainEvent.Email,
            FirstName = domainEvent.FirstName,
            LastName = domainEvent.LastName
        };

        // Add to outbox for reliable publishing via RabbitMQ
        await _outboxRepository.AddAsync(integrationEvent, cancellationToken);

        _logger.LogInformation(
            "Integration event added to outbox for user {UserId} ({Email})",
            domainEvent.UserId,
            domainEvent.Email);
    }
}
