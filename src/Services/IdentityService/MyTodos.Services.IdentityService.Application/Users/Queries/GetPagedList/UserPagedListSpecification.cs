using System.Linq.Expressions;
using MyTodos.BuildingBlocks.Application.Abstractions.Specifications;
using MyTodos.Services.IdentityService.Domain.UserAggregate;

namespace MyTodos.Services.IdentityService.Application.Users.Queries.PagedList;

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