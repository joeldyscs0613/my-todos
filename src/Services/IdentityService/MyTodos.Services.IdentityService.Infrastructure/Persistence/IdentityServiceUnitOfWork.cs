using MyTodos.BuildingBlocks.Application.Contracts.DomainEvents;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions;

namespace MyTodos.Services.IdentityService.Infrastructure.Persistence;

/// <summary>
/// Unit of Work implementation for IdentityService.
/// </summary>
public sealed class IdentityServiceUnitOfWork : UnitOfWork<IdentityServiceDbContext>
{
    public IdentityServiceUnitOfWork(
        IdentityServiceDbContext dbContext,
        ICurrentUserService currentUserService,
        IDomainEventDispatcher domainEventDispatcher)
        : base(dbContext, currentUserService, domainEventDispatcher)
    {
    }
}
