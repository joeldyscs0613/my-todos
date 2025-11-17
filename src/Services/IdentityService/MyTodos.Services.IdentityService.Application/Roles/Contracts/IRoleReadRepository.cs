using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.Services.IdentityService.Domain.RoleAggregate;

namespace MyTodos.Services.IdentityService.Application.Roles.Contracts;

/// <summary>
/// Read repository for Role aggregate.
/// </summary>
public interface IRoleReadRepository : IReadRepository<Role, Guid>
{
    /// <summary>
    /// Get role by code
    /// </summary>
    Task<Role?> GetByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Get all roles matching the input scope.
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<IReadOnlyList<Role>> GetAllByScopeAsync(
        Domain.RoleAggregate.Enums.AccessScope scope, CancellationToken ct = default);
}
