using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.Services.IdentityService.Domain.UserAggregate;

namespace MyTodos.Services.IdentityService.Application.Users.Contracts;

/// <summary>
/// Write repository for User aggregate.
/// </summary>
public interface IUserWriteRepository : IWriteRepository<User, Guid>
{
}
