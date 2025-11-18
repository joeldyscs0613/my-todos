using MyTodos.BuildingBlocks.Application.Contracts.DomainEvents;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions;

namespace MyTodos.Services.TodoService.Infrastructure.Persistence;

public sealed class TodoServiceUnitOfWork : UnitOfWork<TodoServiceDbContext>
{
    public TodoServiceUnitOfWork(
        TodoServiceDbContext dbContext,
        ICurrentUserService currentUserService,
        IDomainEventDispatcher domainEventDispatcher)
        : base(dbContext, currentUserService, domainEventDispatcher)
    {
    }
}
