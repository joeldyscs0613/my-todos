using MyTodos.BuildingBlocks.Application.Abstractions.Queries;
using MyTodos.Services.IdentityService.Application.Users.Contracts;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Users.Queries.GetUserDetails;

/// <summary>
/// Query to get user details by ID.
/// </summary>
public sealed class GetUserDetailsQuery(Guid userId) : Query<UserDetailsResponseDto>
{
    public Guid UserId { get; init; } = userId;
}

/// <summary>
/// User details DTO.
/// </summary>
public sealed record UserDetailsResponseDto
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset? LastLoginAt { get; init; }
    public IReadOnlyList<UserRoleResponseDto> Roles { get; init; } = new List<UserRoleResponseDto>();
}

public sealed record UserRoleResponseDto
{
    public Guid RoleId { get; init; }
    public string RoleName { get; init; } = string.Empty;
    public Guid? TenantId { get; init; }
    public string? TenantName { get; init; }
}

/// <summary>
/// Handler for getting user details.
/// </summary>
public sealed class GetUserDetailsQueryHandler : QueryHandler<GetUserDetailsQuery, UserDetailsResponseDto>
{
    private readonly IUserPagedListReadRepository _userPagedListReadRepository;

    public GetUserDetailsQueryHandler(IUserPagedListReadRepository userPagedListReadRepository)
    {
        _userPagedListReadRepository = userPagedListReadRepository;
    }

    public override async Task<Result<UserDetailsResponseDto>> Handle(GetUserDetailsQuery request, CancellationToken ct)
    {
        var user = await _userPagedListReadRepository.GetByIdAsync(request.UserId, ct);
        if (user == null)
        {
            return NotFound("User not found");
        }

        var dto = new UserDetailsResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            Roles = user.UserRoles.Select(ur => new UserRoleResponseDto
            {
                RoleId = ur.RoleId,
                RoleName = ur.Role.Name,
                TenantId = ur.TenantId,
                TenantName = ur.Tenant?.Name
            }).ToList()
        };

        return Success(dto);
    }
}
