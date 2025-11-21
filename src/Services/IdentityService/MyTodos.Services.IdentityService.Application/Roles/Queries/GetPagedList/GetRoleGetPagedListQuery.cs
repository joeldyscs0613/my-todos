using FluentValidation;
using MyTodos.BuildingBlocks.Application.Abstractions.Queries;
using MyTodos.BuildingBlocks.Application.Validators;
using MyTodos.Services.IdentityService.Application.Roles.Contracts;
using MyTodos.Services.IdentityService.Domain.RoleAggregate;
using MyTodos.Services.IdentityService.Domain.RoleAggregate.Enums;

namespace MyTodos.Services.IdentityService.Application.Roles.Queries.GetPagedList;

/// <summary>
/// Query to get a paged list of roles.
/// </summary>
public sealed class GetRoleGetPagedListQuery
    : GetPagedListQuery<RolePagedListSpecification, RolePagedListFilter, RolePagedListResponseDto>
{
    public GetRoleGetPagedListQuery(RolePagedListFilter filter) : base(filter)
    {
    }
}

/// <summary>
/// Response DTO for role paged list items.
/// </summary>
public sealed record RolePagedListResponseDto(Guid Id, string Code, string Name, int ScopeValue, string ScopeName);

/// <summary>
/// Validator for role paged list queries.
/// </summary>
public sealed class GetRolePagedListQueryValidator
    : GetPagedListQueryValidator<GetRoleGetPagedListQuery, RolePagedListSpecification,
        RolePagedListFilter, RolePagedListResponseDto>
{
    public GetRolePagedListQueryValidator()
    {
        // Add sort field validation
        RuleFor(x => x.Filter.SortField)
            .Must((query, sortField) => BeValidSortField(sortField, query.Specification.ValidSortFields))
            .When(x => x.Filter != null && !string.IsNullOrWhiteSpace(x.Filter.SortField))
            .WithMessage((query, sortField) =>
                GetInvalidSortFieldMessage(sortField!, query.Specification.ValidSortFields));
    }
}

/// <summary>
/// Handler for role paged list queries.
/// </summary>
public sealed class GetRolePagedListQueryHandler(IRolePagedListReadRepository readRepository)
    : GetPagedListQueryHandler<Role, Guid, RolePagedListSpecification, RolePagedListFilter,
        GetRoleGetPagedListQuery, RolePagedListResponseDto>(readRepository)
{
    protected override List<RolePagedListResponseDto> GetResultList(
        GetRoleGetPagedListQuery request, IReadOnlyList<Role> list)
    {
        return list.Select(r
            => new RolePagedListResponseDto(r.Id, r.Code, r.Name, (int)r.Scope, r.Scope.ToString()))
            .ToList();
    }
}
