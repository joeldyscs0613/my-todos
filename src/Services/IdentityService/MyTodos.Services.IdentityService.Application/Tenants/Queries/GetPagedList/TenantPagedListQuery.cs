using MyTodos.BuildingBlocks.Application.Abstractions.Queries;
using MyTodos.Services.IdentityService.Application.Users.Contracts;
using MyTodos.Services.IdentityService.Application.Users.Queries.GetPagedList;
using MyTodos.Services.IdentityService.Domain.UserAggregate;

namespace MyTodos.Services.IdentityService.Application.Tenants.Queries.GetPagedList;

public sealed class UserPagedListQuery 
    : PagedListQuery<UserPagedListSpecification, UserPagedListFilter, TenantPagedListResponseDto>
{
}

public sealed class UserPagedListQueryHandler(IUserPagedListReadRepository readRepository)
    : PagedListQueryHandler<User, Guid, UserPagedListSpecification, UserPagedListFilter,
        UserPagedListQuery, TenantPagedListResponseDto>(readRepository)
{
    protected override List<TenantPagedListResponseDto> GetResultList(
        UserPagedListQuery request, IReadOnlyList<User> list)
    {
        return list.Select(u 
            => new TenantPagedListResponseDto(u.Id, u.FirstName, u.LastName, u.Username, u.Email, u.IsActive))
            .ToList();
    }
}