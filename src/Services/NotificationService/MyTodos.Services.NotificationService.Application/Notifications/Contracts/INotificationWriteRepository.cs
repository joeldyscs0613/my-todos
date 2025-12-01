using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.Services.NotificationService.Domain.NotificationAggregate;

namespace MyTodos.Services.NotificationService.Application.Notifications.Contracts;

/// <summary>
/// Write repository for Notification aggregate.
/// </summary>
public interface INotificationWriteRepository : IWriteRepository<Notification, Guid>
{
}
