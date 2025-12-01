using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Repositories;
using MyTodos.Services.NotificationService.Application.Notifications.Contracts;
using MyTodos.Services.NotificationService.Application.Notifications.Queries;
using MyTodos.Services.NotificationService.Domain.NotificationAggregate;

namespace MyTodos.Services.NotificationService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Write repository implementation for Notification aggregate.
/// </summary>
public sealed class NotificationWriteRepository(
    NotificationDbContext context,
    ICurrentUserService currentUserService)
    : WriteEfRepository<Notification, Guid, NotificationDbContext>(
        context,
        new NotificationQueryConfiguration(),
        currentUserService),
      INotificationWriteRepository
{
}
