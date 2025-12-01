using MyTodos.BuildingBlocks.Application.Contracts.Queries;
using MyTodos.Services.NotificationService.Domain.NotificationAggregate;

namespace MyTodos.Services.NotificationService.Application.Notifications.Queries;

/// <summary>
/// Query configuration for Notification entity.
/// </summary>
public sealed class NotificationQueryConfiguration : IEntityQueryConfiguration<Notification>
{
    public IQueryable<Notification> ConfigureAggregate(IQueryable<Notification> query)
    {
        // No related entities to include for Notification
        return query;
    }
}
