namespace MyTodos.Services.IdentityService.Application.Tenants.Queries.GetTenantDetails;

public sealed record TenantDetailsDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Plan { get; init; } = string.Empty;
    public int MaxUsers { get; init; }
    public bool IsActive { get; init; }
    public DateTime? SubscriptionExpiresAt { get; init; }
}
