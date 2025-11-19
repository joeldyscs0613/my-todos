using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MyTodos.BuildingBlocks.Application.Abstractions.Filters;
using MyTodos.BuildingBlocks.Application.Abstractions.Queries;
using MyTodos.BuildingBlocks.Application.Abstractions.Specifications;
using MyTodos.BuildingBlocks.Application.Contracts.Queries;
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

public sealed record UserPagedListResponseDto(
    Guid Id, string FirstName, string LastName, string Username, string Email, bool IsActive);

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

public sealed class UserPagedListSpecification(UserPagedListFilter filter)
    : Specification<User, Guid, UserPagedListFilter>(filter)
{
    protected override IQueryable<User> ApplyFilter(IQueryable<User> query)
    {
        if (Filter.Username != null && Filter.UserId != Guid.Empty)
        {
            query = query.Where(u => u.Id == Filter.UserId);
        }

        if (!string.IsNullOrWhiteSpace(Filter.Username))
        {
            query = query.Where(u => u.Username == Filter.Username);
        }

        if (!string.IsNullOrWhiteSpace(Filter.Email))
        {
            query = query.Where(u => u.Email == Filter.Email);
        }

        if (!string.IsNullOrWhiteSpace(Filter.FirstName))
        {
            query = query.Where(u => u.FirstName == Filter.FirstName);
        }

        if (!string.IsNullOrWhiteSpace(Filter.LastName))
        {
            query = query.Where(u => u.LastName == Filter.LastName);
        }

        if (Filter.TenantId != null && Filter.TenantId != Guid.Empty)
        {
            query = query.Where(u => u.UserRoles.Any(ur => ur.TenantId == Filter.TenantId));
        }

        if (Filter.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == Filter.IsActive);
        }

        return query;
    }

    protected override IQueryable<User> ApplySearchBy(IQueryable<User> query)
    {
        if (!string.IsNullOrWhiteSpace(Filter.SearchBy))
        {
            query.Where(u => u.Username.Contains(Filter.SearchBy)
                             || u.Email.Contains(Filter.SearchBy)
                             || u.FirstName.Contains(Filter.SearchBy)
                             || u.LastName.Contains(Filter.SearchBy));
        }

        return query;
    }

    protected override Dictionary<string, Expression<Func<User, object>>> GetSortFunctions()
    {
        return new Dictionary<string, Expression<Func<User, object>>>
        {
            { nameof(Filter.Username), u => u.Username },
            { nameof(Filter.Email), u => u.Email },
            { nameof(Filter.FirstName), u => u.FirstName },
            { nameof(Filter.LastName), u => u.LastName }
        };
    }
}

public sealed class UserQueryConfiguration : IEntityQueryConfiguration<User>
{
    public IQueryable<User> ConfigureAggregate(IQueryable<User> query)
    {
        return query
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission);
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
