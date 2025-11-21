using System.Linq.Expressions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyTodos.BuildingBlocks.Application.Abstractions.Filters;
using MyTodos.BuildingBlocks.Application.Abstractions.Queries;
using MyTodos.BuildingBlocks.Application.Abstractions.Specifications;
using MyTodos.BuildingBlocks.Application.Contracts.Queries;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.BuildingBlocks.Application.Helpers;
using MyTodos.BuildingBlocks.Application.Validators;
using MyTodos.Services.IdentityService.Application.Tenants.Contracts;
using MyTodos.Services.IdentityService.Domain.TenantAggregate;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Tenants.Queries.GetPagedList;

public sealed class GetTenantPagedListQuery
    : GetPagedListQuery<TenantPagedListSpecification, TenantPagedListFilter, TenantPagedListResponseDto>
{
    public GetTenantPagedListQuery(TenantPagedListFilter filter) : base(filter)
    {
    }
}

public sealed record TenantPagedListResponseDto(Guid Id, string Name, bool IsActive);

public sealed class TenantPagedListFilter : Filter
{
    public string? Name { get; set; }
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
            { nameof(Filter.IsActive), t => t.IsActive }
        };
    }
}

public sealed class TenantQueryConfiguration : IEntityQueryConfiguration<Tenant>
{
    public IQueryable<Tenant> ConfigureAggregate(IQueryable<Tenant> query)
    {
        // Simplified to avoid circular includes
        return query;
    }
}

public sealed class TenantGetPagedListQueryValidator
    : GetPagedListQueryValidator<GetTenantPagedListQuery, TenantPagedListSpecification,
        TenantPagedListFilter, TenantPagedListResponseDto>
{
    public TenantGetPagedListQueryValidator()
    {
        // Add sort field validation
        RuleFor(x => x.Filter.SortField)
            .Must((query, sortField) => BeValidSortField(sortField, query.Specification.ValidSortFields))
            .When(x => x.Filter != null && !string.IsNullOrWhiteSpace(x.Filter.SortField))
            .WithMessage((query, sortField) =>
                GetInvalidSortFieldMessage(sortField!, query.Specification.ValidSortFields));
    }
}

public sealed class TenantGetPagedListQueryHandler(
    ITenantPagedListReadRepository readRepository,
    ICurrentUserService currentUserService)
    : GetPagedListQueryHandler<Tenant, Guid, TenantPagedListSpecification, TenantPagedListFilter,
        GetTenantPagedListQuery, TenantPagedListResponseDto>(readRepository)
{
    public override async Task<Result<PagedList<TenantPagedListResponseDto>>> Handle(
        GetTenantPagedListQuery request,
        CancellationToken ct)
    {
        // Only Global Administrators can view tenant lists
        if (!currentUserService.IsGlobalAdmin())
        {
            return Result.Forbidden<PagedList<TenantPagedListResponseDto>>(
                "Only Global Administrators can view tenant lists");
        }

        return await base.Handle(request, ct);
    }

    protected override List<TenantPagedListResponseDto> GetResultList(
        GetTenantPagedListQuery request, IReadOnlyList<Tenant> list)
    {
        return list.Select(t
            => new TenantPagedListResponseDto(t.Id, t.Name, t.IsActive))
            .ToList();
    }
}
