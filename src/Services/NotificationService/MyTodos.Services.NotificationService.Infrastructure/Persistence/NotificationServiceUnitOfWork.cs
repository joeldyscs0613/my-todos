using MyTodos.BuildingBlocks.Application.Contracts.DomainEvents;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions;

namespace MyTodos.Services.NotificationService.Infrastructure.Persistence;

public sealed class NotificationServiceUnitOfWork : UnitOfWork<NotificationServiceDbContext>
{
    public NotificationServiceUnitOfWork(
        NotificationServiceDbContext dbContext,
        ICurrentUserService currentUserService,
        IDomainEventDispatcher domainEventDispatcher)
        : base(dbContext, currentUserService, domainEventDispatcher)
    {
    }
}
