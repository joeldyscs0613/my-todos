using MyTodos.SharedKernel.Abstractions;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.NotificationService.Domain.NotificationAggregate;

/// <summary>
/// Represents a notification sent to a user.
/// </summary>
public sealed class Notification : AggregateRoot<Guid>
{
    // Private parameterless constructor for EF Core
    [Obsolete("Only for deserialization. Use Create method.", error: true)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    private Notification() : base() { }

    // Constructor for factory method
    private Notification(Guid id,
        Guid userId,
        string email,
        NotificationType type,
        string subject,
        string body) : base(id)     
    {
        UserId = userId;
        Email = email;
        Type = type;
        Subject = subject;
        Body = body;
        Status = NotificationStatus.Pending;
        SentAt = null;
        FailureReason = null;
        RetryCount = 0;
    }

    /// <summary>
    /// ID of the user who received the notification.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Email address where the notification was sent.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Type of notification.
    /// </summary>
    public NotificationType Type { get; private set; }

    /// <summary>
    /// Email subject line.
    /// </summary>
    public string Subject { get; private set; } = string.Empty;

    /// <summary>
    /// Email body content.
    /// </summary>
    public string Body { get; private set; } = string.Empty;

    /// <summary>
    /// Current status of the notification.
    /// </summary>
    public NotificationStatus Status { get; private set; }

    /// <summary>
    /// Timestamp when the notification was successfully sent.
    /// </summary>
    public DateTimeOffset? SentAt { get; private set; }

    /// <summary>
    /// Reason for failure if the notification failed to send.
    /// </summary>
    public string? FailureReason { get; private set; }

    /// <summary>
    /// Number of times delivery has been retried.
    /// </summary>
    public int RetryCount { get; private set; }

    /// <summary>
    /// Creates a new notification.
    /// </summary>
    public static Result<Notification> Create(
        Guid userId,
        string email,
        NotificationType type,
        string subject,
        string body)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure<Notification>(Error.BadRequest("User ID cannot be empty"));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<Notification>(Error.BadRequest("Email is required"));
        }

        if (string.IsNullOrWhiteSpace(subject))
        {
            return Result.Failure<Notification>(Error.BadRequest("Subject is required"));
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            return Result.Failure<Notification>(Error.BadRequest("Body is required"));
        }

        var notificationId = Guid.NewGuid();
        var notification = new Notification(notificationId, userId, email, type, subject, body);

        return Result.Success(notification);
    }

    /// <summary>
    /// Marks the notification as successfully sent.
    /// </summary>
    public Result MarkAsSent()
    {
        if (Status == NotificationStatus.Sent)
        {
            return Result.Failure(Error.Conflict("Notification has already been marked as sent"));
        }

        Status = NotificationStatus.Sent;
        SentAt = DateTimeOffset.UtcNow;
        FailureReason = null;

        return Result.Success();
    }

    /// <summary>
    /// Marks the notification as failed with a reason.
    /// </summary>
    public Result MarkAsFailed(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure(Error.BadRequest("Failure reason is required"));
        }

        Status = NotificationStatus.Failed;
        FailureReason = reason;

        return Result.Success();
    }

    /// <summary>
    /// Marks the notification for retry.
    /// </summary>
    public Result MarkForRetry()
    {
        if (Status == NotificationStatus.Sent)
        {
            return Result.Failure(Error.Conflict("Cannot retry a notification that has already been sent"));
        }

        Status = NotificationStatus.Retrying;
        RetryCount++;

        return Result.Success();
    }
}
