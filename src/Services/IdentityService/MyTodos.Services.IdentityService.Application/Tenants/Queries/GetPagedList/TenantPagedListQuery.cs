using System.Linq.Expressions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyTodos.BuildingBlocks.Application.Abstractions.Filters;
using MyTodos.BuildingBlocks.Application.Abstractions.Queries;
using MyTodos.BuildingBlocks.Application.Abstractions.Specifications;
using MyTodos.BuildingBlocks.Application.Contracts.Queries;
using MyTodos.BuildingBlocks.Application.Validators;
using MyTodos.Services.IdentityService.Application.Tenants.Contracts;
using MyTodos.Services.IdentityService.Domain.TenantAggregate;
using MyTodos.Services.IdentityService.Domain.TenantAggregate.Enums;

namespace MyTodos.Services.IdentityService.Application.Tenants.Queries.GetPagedList;

public sealed class TenantPagedListQuery
    : PagedListQuery<TenantPagedListSpecification, TenantPagedListFilter, TenantPagedListResponseDto>
{
    public TenantPagedListQuery(TenantPagedListFilter filter) : base(filter)
    {
    }
}

public sealed record TenantPagedListResponseDto(Guid Id, string Name, string TenantPlan, bool IsActive);

public sealed class TenantPagedListFilter : Filter
{
    public string? Name { get; set; }
    public string? TenantPlan { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class TenantPagedListSpecification(TenantPagedListFilter filter)
    : Specification<Tenant, Guid, TenantPagedListFilter>(filter)
{
    protected override IQueryable<Tenant> ApplyFilter(IQueryable<Tenant> query)
    {
        if (!string.IsNullOrWhiteSpace(Filter.Name))
        {
            query = query.Where(t => t.Name.Contains(Filter.Name));
        }

        return query;
    }

    protected override IQueryable<Tenant> ApplySearchBy(IQueryable<Tenant> query)
    {
        if (!string.IsNullOrWhiteSpace(Filter.SearchBy))
        {
            query = query.Where(t => t.Name.Contains(Filter.SearchBy));
        }

        return query;
    }

    protected override Dictionary<string, Expression<Func<Tenant, object>>> GetSortFunctions()
    {
        return new Dictionary<string, Expression<Func<Tenant, object>>>
        {
            { nameof(Filter.Name), t => t.Name },
            { nameof(Filter.TenantPlan), t => t.Plan },
            { nameof(Filter.IsActive), t => t.IsActive }
        };
    }
}

public sealed class TenantQueryConfiguration : IEntityQueryConfiguration<Tenant>
{
    public IQueryable<Tenant> ConfigureAggregate(IQueryable<Tenant> query)
    {
        return query
            .Include(t => t.UserRoles)
            .ThenInclude(ur => ur.User)
            .ThenInclude(t => t.UserRoles)
            .ThenInclude(ur => ur.Role);
    }
}

public sealed class TenantPagedListQueryValidator
    : PagedListQueryValidator<TenantPagedListQuery, TenantPagedListSpecification,
        TenantPagedListFilter, TenantPagedListResponseDto>
{
    public TenantPagedListQueryValidator()
    {
        // Add sort field validation
        RuleFor(x => x.Filter.SortField)
            .Must((query, sortField) => BeValidSortField(sortField, query.Specification.ValidSortFields))
            .When(x => x.Filter != null && !string.IsNullOrWhiteSpace(x.Filter.SortField))
            .WithMessage((query, sortField) =>
                GetInvalidSortFieldMessage(sortField!, query.Specification.ValidSortFields));
    }
}

public sealed class TenantPagedListQueryHandler(ITenantPagedListReadRepository readRepository)
    : PagedListQueryHandler<Tenant, Guid, TenantPagedListSpecification, TenantPagedListFilter,
        TenantPagedListQuery, TenantPagedListResponseDto>(readRepository)
{
    protected override List<TenantPagedListResponseDto> GetResultList(
        TenantPagedListQuery request, IReadOnlyList<Tenant> list)
    {
        return list.Select(t
            => new TenantPagedListResponseDto(t.Id, t.Name, Enum.GetName(typeof(TenantPlan), t.Plan), t.IsActive))
            .ToList();
    }
}
