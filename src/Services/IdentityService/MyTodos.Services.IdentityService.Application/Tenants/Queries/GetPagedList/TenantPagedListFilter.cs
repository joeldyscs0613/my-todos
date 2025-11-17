using MyTodos.BuildingBlocks.Application.Abstractions.Filters;

namespace MyTodos.Services.IdentityService.Application.Tenants.Queries.GetPagedList;

public sealed class TenantPagedListFilter : Filter
{
    public string? Name { get; set; }
    public string? TenantPlan { get; set; }
    public bool? IsActive { get; set; }
}