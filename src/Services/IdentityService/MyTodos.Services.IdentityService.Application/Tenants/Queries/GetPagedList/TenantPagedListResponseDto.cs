namespace MyTodos.Services.IdentityService.Application.Tenants.Queries.GetPagedList;

public sealed record TenantPagedListResponseDto(Guid Id, string Name, string TenantPlan, bool IsActive);