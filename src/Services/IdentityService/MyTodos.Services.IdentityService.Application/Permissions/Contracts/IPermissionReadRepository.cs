using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.Services.IdentityService.Domain.PermissionAggregate;

namespace MyTodos.Services.IdentityService.Application.Permissions.Contracts;

/// <summary>
/// Read repository for Permission aggregate.
/// </summary>
public interface IPermissionReadRepository : IReadRepository<Permission, Guid>
{
    /// <summary>
    /// Get permission by code
    /// </summary>
    Task<Permission?> GetByCodeAsync(string code, CancellationToken ct = default);
}
