using MyTodos.BuildingBlocks.Application.Abstractions.Queries;
using MyTodos.Services.IdentityService.Domain.UserAggregate.Contracts;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Features.Users.Queries.GetUserDetails;

/// <summary>
/// Handler for getting user details.
/// </summary>
public sealed class GetUserDetailsQueryHandler : QueryHandler<GetUserDetailsQuery, UserDetailsDto>
{
    private readonly IUserReadRepository _userReadRepository;

    public GetUserDetailsQueryHandler(IUserReadRepository userReadRepository)
    {
        _userReadRepository = userReadRepository;
    }

    public override async Task<Result<UserDetailsDto>> Handle(GetUserDetailsQuery request, CancellationToken ct)
    {
        var user = await _userReadRepository.GetByIdWithRolesAsync(request.UserId, ct);
        if (user == null)
        {
            return NotFound("User not found");
        }

        var dto = new UserDetailsDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            Roles = user.UserRoles.Select(ur => new UserRoleDto
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
