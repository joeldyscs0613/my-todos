using MyTodos.BuildingBlocks.Application.Abstractions.Queries;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.BuildingBlocks.Application.Helpers;
using MyTodos.Services.IdentityService.Application.Users.Contracts;
using MyTodos.Services.IdentityService.Domain.UserAggregate;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Users.Queries.GetPagedList;

public sealed class UserGetPagedListQuery
    : GetPagedListQuery<UserPagedListSpecification, UserPagedListFilter, UserPagedListResponseDto>
{
    public UserGetPagedListQuery(UserPagedListFilter filter) : base(filter)
    {
    }
}

public sealed record UserPagedListResponseDto(
    Guid Id, string FirstName, string LastName, string Username, string Email, bool IsActive);

public sealed class UserGetPagedListQueryHandler(
    IUserPagedListReadRepository readRepository,
    ICurrentUserService currentUserService)
    : GetPagedListQueryHandler<User, Guid, UserPagedListSpecification, UserPagedListFilter,
        UserGetPagedListQuery, UserPagedListResponseDto>(readRepository)
{
    public override async Task<Result<PagedList<UserPagedListResponseDto>>> Handle(
        UserGetPagedListQuery request,
        CancellationToken ct)
    {
        // Apply tenant filtering for non-Global Admins
        if (!currentUserService.IsGlobalAdmin())
        {
            var tenantId = currentUserService.TenantId;
            if (!tenantId.HasValue)
            {
                return Result.Forbidden<PagedList<UserPagedListResponseDto>>(
                    "No tenant context found");
            }

            // Force tenant filtering to show only users in current user's tenant
            request.Filter.TenantId = tenantId.Value;
        }

        return await base.Handle(request, ct);
    }

    protected override List<UserPagedListResponseDto> GetResultList(
        UserGetPagedListQuery request, IReadOnlyList<User> list)
    {
        return list.Select(u
            => new UserPagedListResponseDto(u.Id, u.FirstName, u.LastName, u.Username, u.Email, u.IsActive))
            .ToList();
    }
}
