using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.Services.IdentityService.Domain.UserAggregate.DomainEvents;

/// <summary>
/// Domain event raised when a user logs in
/// </summary>
public sealed record UserLoggedInDomainEvent : DomainEvent
{
    public Guid UserId { get; init; }
    public DateTimeOffset LoginTimestamp { get; init; }

    public UserLoggedInDomainEvent(Guid userId, DateTimeOffset loginTimestamp)
        : base("UserLoggedIn", "User", userId.ToString())
    {
        UserId = userId;
        LoginTimestamp = loginTimestamp;
    }

    // For deserialization
    [Obsolete("Only for deserialization. Use parameterized constructor.", error: true)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    private UserLoggedInDomainEvent() : base() { }
}
