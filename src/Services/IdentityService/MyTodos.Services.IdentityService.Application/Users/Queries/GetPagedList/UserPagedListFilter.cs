using MyTodos.BuildingBlocks.Application.Abstractions.Filters;

namespace MyTodos.Services.IdentityService.Application.Users.Queries.GetPagedList;

public sealed class UserPagedListFilter : Filter
{
    public Guid? UserId { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public Guid? TenantId { get; set; }

    public bool? IsActive { get; set; }
}