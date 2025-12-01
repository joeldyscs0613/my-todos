namespace MyTodos.Services.NotificationService.Domain.NotificationAggregate;

/// <summary>
/// Types of notifications that can be sent.
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Welcome email sent to new users.
    /// </summary>
    WelcomeEmail = 10,

    /// <summary>
    /// Password reset notification.
    /// </summary>
    PasswordReset = 20,

    /// <summary>
    /// Email verification notification.
    /// </summary>
    EmailVerification = 30,

    /// <summary>
    /// General system notification.
    /// </summary>
    SystemNotification = 40
}
