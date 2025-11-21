using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.Services.IdentityService.Domain.TenantAggregate.DomainEvents;

/// <summary>
/// Domain event raised when a tenant is deleted.
/// This allows other parts of the system to react to tenant deletion
/// (e.g., delete associated users, clean up resources, etc.)
/// </summary>
public sealed record TenantDeletedDomainEvent(
    Guid TenantId,
    string Name) : DomainEvent(
    nameof(TenantDeletedDomainEvent),
    nameof(Tenant),
    TenantId.ToString());
