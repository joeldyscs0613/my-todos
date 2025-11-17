using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.Services.IdentityService.Domain.UserAggregate.DomainEvents;

/// <summary>
/// Domain event raised when a new user registers
/// </summary>
public sealed record UserRegisteredDomainEvent : DomainEvent
{
    public Guid UserId { get; init; }
    public string Username { get; init; }
    public string Email { get; init; }

    public UserRegisteredDomainEvent(Guid userId, string username, string email)
        : base("UserRegistered", "User", userId.ToString())
    {
        UserId = userId;
        Username = username;
        Email = email;
    }

    // For deserialization
    [Obsolete("Only for deserialization. Use parameterized constructor.", error: true)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    private UserRegisteredDomainEvent() : base() { }
}
