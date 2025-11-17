using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.Services.IdentityService.Domain.TenantAggregate.Enums;

namespace MyTodos.Services.IdentityService.Application.Tenants.Commands.CreateTenant;

/// <summary>
/// Command to create a new tenant (Global Admin only).
/// </summary>
public sealed class CreateTenantCommand : CreateCommand<Guid>
{
    public string Name { get; init; } = string.Empty;
    public TenantPlan Plan { get; init; } = TenantPlan.Free;
    public int MaxUsers { get; init; } = 5;
}
