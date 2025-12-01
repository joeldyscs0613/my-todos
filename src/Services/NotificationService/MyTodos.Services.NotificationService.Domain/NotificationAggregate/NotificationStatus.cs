namespace MyTodos.Services.NotificationService.Domain.NotificationAggregate;

/// <summary>
/// Status of a notification delivery.
/// </summary>
public enum NotificationStatus
{
    /// <summary>
    /// Notification is pending delivery.
    /// </summary>
    Pending = 10,

    /// <summary>
    /// Notification was successfully sent.
    /// </summary>
    Sent = 20,

    /// <summary>
    /// Notification delivery failed.
    /// </summary>
    Failed = 30,

    /// <summary>
    /// Notification delivery is being retried.
    /// </summary>
    Retrying = 40
}
