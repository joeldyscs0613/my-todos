using Microsoft.EntityFrameworkCore;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Repositories;
using MyTodos.Services.NotificationService.Application.Notifications.Contracts;
using MyTodos.Services.NotificationService.Application.Notifications.Queries;
using MyTodos.Services.NotificationService.Domain.NotificationAggregate;

namespace MyTodos.Services.NotificationService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Read repository implementation for Notification aggregate.
/// </summary>
public sealed class NotificationReadRepository(
    NotificationDbContext context,
    ICurrentUserService currentUserService)
    : ReadEfRepository<Notification, Guid, NotificationDbContext>(
        context,
        new NotificationQueryConfiguration(),
        currentUserService),
      INotificationReadRepository
{
    public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await GetInitialQueryForList()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedDate)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Notification>> GetByStatusAsync(
        NotificationStatus status, CancellationToken ct = default)
    {
        return await GetInitialQueryForList()
            .Where(n => n.Status == status)
            .OrderByDescending(n => n.CreatedDate)
            .ToListAsync(ct);
    }
}
