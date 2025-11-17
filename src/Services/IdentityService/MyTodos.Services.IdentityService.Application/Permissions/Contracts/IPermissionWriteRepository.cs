using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.Services.IdentityService.Domain.PermissionAggregate;

namespace MyTodos.Services.IdentityService.Application.Permissions.Contracts;

/// <summary>
/// Write repository for Permission aggregate.
/// </summary>
public interface IPermissionWriteRepository : IWriteRepository<Permission, Guid>
{
}
