using MyTodos.BuildingBlocks.Application.Abstractions.Queries;

namespace MyTodos.Services.IdentityService.Application.Features.Users.Queries.GetUserDetails;

/// <summary>
/// Query to get user details by ID.
/// </summary>
public sealed class GetUserDetailsQuery(Guid userId) : Query<UserDetailsDto>
{
    public Guid UserId { get; init; } = userId;
}
