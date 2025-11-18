using MyTodos.BuildingBlocks.Application.Abstractions.Queries;
using MyTodos.Services.IdentityService.Application.Users.Contracts;
using MyTodos.Services.IdentityService.Domain.UserAggregate;

namespace MyTodos.Services.IdentityService.Application.Users.Queries.GetPagedList;

public sealed class UserPagedListQuery
    : PagedListQuery<UserPagedListSpecification, UserPagedListFilter, UserPagedListResponseDto>
{
    public UserPagedListQuery(UserPagedListFilter filter) : base(filter)
    {
    }
}

public sealed class UserPagedListQueryHandler(IUserPagedListReadRepository readRepository)
    : PagedListQueryHandler<User, Guid, UserPagedListSpecification, UserPagedListFilter,
        UserPagedListQuery, UserPagedListResponseDto>(readRepository)
{
    protected override List<UserPagedListResponseDto> GetResultList(
        UserPagedListQuery request, IReadOnlyList<User> list)
    {
        return list.Select(u 
            => new UserPagedListResponseDto(u.Id, u.FirstName, u.LastName, u.Username, u.Email, u.IsActive))
            .ToList();
    }
}