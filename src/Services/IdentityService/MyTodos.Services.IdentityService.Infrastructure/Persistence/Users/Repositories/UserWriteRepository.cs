using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Repositories;
using MyTodos.Services.IdentityService.Application.Users.Contracts;
using MyTodos.Services.IdentityService.Application.Users.Queries.GetPagedList;
using MyTodos.Services.IdentityService.Domain.UserAggregate;

namespace MyTodos.Services.IdentityService.Infrastructure.Persistence.Users.Repositories;

/// <summary>
/// Write repository for User aggregate mutations.
/// </summary>
public sealed class UserWriteRepository(IdentityServiceDbContext context, ICurrentUserService currentUserService)
    : WriteEfRepository<User, Guid, IdentityServiceDbContext>(context, new UserQueryConfiguration(), currentUserService),
        IUserWriteRepository
{
}
