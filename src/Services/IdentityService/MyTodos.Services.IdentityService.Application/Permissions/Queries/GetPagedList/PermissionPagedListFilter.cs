using MyTodos.BuildingBlocks.Application.Abstractions.Filters;

namespace MyTodos.Services.IdentityService.Application.Permissions.Queries.GetPagedList;

/// <summary>
/// Filter for permission paged list queries.
/// </summary>
public sealed class PermissionPagedListFilter : Filter
{
    public string? Code { get; set; }
    public string? Name { get; set; }
}
