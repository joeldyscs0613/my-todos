using MyTodos.BuildingBlocks.Application.Contracts.DomainEvents;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions;

namespace MyTodos.Services.NotificationService.Infrastructure.Persistence;

public sealed class NotificationServiceUnitOfWork : UnitOfWork<NotificationDbContext>
{
    public NotificationServiceUnitOfWork(
        NotificationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDomainEventDispatcher domainEventDispatcher)
        : base(dbContext, currentUserService, domainEventDispatcher)
    {
    }
}
