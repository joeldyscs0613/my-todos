using MyTodos.BuildingBlocks.Application.Abstractions.IntegrationEvents;

namespace MyTodos.Services.IdentityService.Contracts.IntegrationEvents;

/// <summary>
/// Integration event published when a new user is created in IdentityService.
/// This event is consumed by other services (e.g., NotificationService).
/// </summary>
public sealed record UserCreatedIntegrationEvent : IntegrationEvent
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
}
