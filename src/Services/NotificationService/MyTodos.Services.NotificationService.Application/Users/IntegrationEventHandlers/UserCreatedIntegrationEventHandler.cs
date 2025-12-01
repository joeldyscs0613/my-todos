using Microsoft.Extensions.Logging;
using MyTodos.BuildingBlocks.Application.Contracts.IntegrationEvents;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.Services.IdentityService.Contracts.IntegrationEvents;
using MyTodos.Services.NotificationService.Application.Common.Contracts;
using MyTodos.Services.NotificationService.Application.Notifications.Contracts;
using MyTodos.Services.NotificationService.Domain.NotificationAggregate;

namespace MyTodos.Services.NotificationService.Application.Users.IntegrationEventHandlers;

/// <summary>
/// Handles UserCreatedIntegrationEvent by sending a welcome email to the newly created user.
/// </summary>
public sealed class UserCreatedIntegrationEventHandler
    : IIntegrationEventHandler<UserCreatedIntegrationEvent>
{
    private readonly IEmailService _emailService;
    private readonly INotificationWriteRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserCreatedIntegrationEventHandler> _logger;

    public UserCreatedIntegrationEventHandler(
        IEmailService emailService,
        INotificationWriteRepository notificationRepository,
        IUnitOfWork unitOfWork,
        ILogger<UserCreatedIntegrationEventHandler> logger)
    {
        _emailService = emailService;
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task HandleAsync(
        UserCreatedIntegrationEvent integrationEvent,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Handling UserCreatedIntegrationEvent for user {UserId} ({Email})",
            integrationEvent.UserId,
            integrationEvent.Email);

        // Create notification record
        var displayName = !string.IsNullOrWhiteSpace(integrationEvent.FirstName) 
                          || !string.IsNullOrWhiteSpace(integrationEvent.LastName)
            ? $"{integrationEvent.FirstName} {integrationEvent.LastName}".Trim()
            : integrationEvent.Username;

        var subject = "Welcome to MyTodos!";
        var body = $@"Hello {displayName},

            Welcome to MyTodos! We're excited to have you on board.

            Your account has been successfully created with the following details:
            - Username: {integrationEvent.Username}

            You can now log in and start managing your tasks and projects.

            If you have any questions or need assistance, please don't hesitate to reach out to our support team.

            Best regards,
            The MyTodos Team";

        var notificationResult = Notification.Create(
            integrationEvent.UserId,
            integrationEvent.Email,
            NotificationType.WelcomeEmail,
            subject,
            body);

        if (notificationResult.IsFailure)
        {
            _logger.LogError(
                "Failed to create notification record for user {UserId}: {Error}",
                integrationEvent.UserId,
                notificationResult.FirstError.Description);
            throw new InvalidOperationException(
                $"Failed to create notification: {notificationResult.FirstError.Description}");
        }

        var notification = notificationResult.Value!;

        try
        {
            // Save notification as pending
            await _notificationRepository.AddAsync(notification, ct);
            await _unitOfWork.CommitAsync(ct);

            _logger.LogInformation(
                "Created notification record {NotificationId} for user {UserId}",
                notification.Id,
                integrationEvent.UserId);

            // Send welcome email
            var emailSent = await _emailService.SendEmailAsync(
                integrationEvent.Email,
                subject,
                body,
                ct);

            if (emailSent)
            {
                notification.MarkAsSent();
                _logger.LogInformation(
                    "Successfully sent welcome email to user {UserId} ({Email})",
                    integrationEvent.UserId,
                    integrationEvent.Email);
            }
            else
            {
                notification.MarkAsFailed("Email service returned false");
                _logger.LogWarning(
                    "Failed to send welcome email to user {UserId} ({Email})",
                    integrationEvent.UserId,
                    integrationEvent.Email);
            }

            // Update notification status
            await _unitOfWork.CommitAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error handling UserCreatedIntegrationEvent for user {UserId} ({Email})",
                integrationEvent.UserId,
                integrationEvent.Email);

            // Mark notification as failed
            notification.MarkAsFailed(ex.Message);
            await _unitOfWork.CommitAsync(ct);

            // Rethrow to trigger retry logic in consumer
            throw;
        }
    }
}
