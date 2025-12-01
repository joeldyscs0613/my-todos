using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.Services.NotificationService.Domain.NotificationAggregate;

namespace MyTodos.Services.NotificationService.Application.Notifications.Contracts;

/// <summary>
/// Read repository for Notification aggregate.
/// </summary>
public interface INotificationReadRepository : IReadRepository<Notification, Guid>
{
    /// <summary>
    /// Gets notifications by user ID.
    /// </summary>
    Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Gets notifications by status.
    /// </summary>
    Task<IEnumerable<Notification>> GetByStatusAsync(NotificationStatus status, CancellationToken ct = default);
}
