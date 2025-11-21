using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.Services.IdentityService.Domain.UserAggregate.DomainEvents;

/// <summary>
/// Domain event raised when a new user is created.
/// This is internal to IdentityService and will be converted to an integration event.
/// </summary>
public sealed record UserCreatedDomainEvent : DomainEvent
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;

    public UserCreatedDomainEvent(
        Guid userId,
        string username,
        string email,
        string firstName,
        string lastName)
        : base("UserCreated", "User", userId.ToString())
    {
        UserId = userId;
        Username = username;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
    }
}
