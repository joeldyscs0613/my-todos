using MyTodos.BuildingBlocks.Application.Abstractions.Filters;
using MyTodos.Services.IdentityService.Domain.RoleAggregate.Enums;

namespace MyTodos.Services.IdentityService.Application.Roles.Queries.GetPagedList;

/// <summary>
/// Filter for role paged list queries.
/// </summary>
public sealed class RolePagedListFilter : Filter
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public AccessScope? Scope { get; set; }
}
