using FluentValidation;
using MyTodos.BuildingBlocks.Application.Abstractions.Queries;
using MyTodos.BuildingBlocks.Application.Validators;
using MyTodos.Services.IdentityService.Application.Permissions.Contracts;
using MyTodos.Services.IdentityService.Domain.PermissionAggregate;

namespace MyTodos.Services.IdentityService.Application.Permissions.Queries.GetPagedList;

/// <summary>
/// Query to get a paged list of permissions.
/// </summary>
public sealed class GetPermissionPagedListQuery
    : GetPagedListQuery<PermissionPagedListSpecification, PermissionPagedListFilter, PermissionPagedListResponseDto>
{
    public GetPermissionPagedListQuery(PermissionPagedListFilter filter) : base(filter)
    {
    }
}

/// <summary>
/// Response DTO for permission paged list items.
/// </summary>
public sealed record PermissionPagedListResponseDto(Guid Id, string Code, string Name);

/// <summary>
/// Validator for permission paged list queries.
/// </summary>
public sealed class GetPermissionPagedListQueryValidator
    : GetPagedListQueryValidator<GetPermissionPagedListQuery, PermissionPagedListSpecification,
        PermissionPagedListFilter, PermissionPagedListResponseDto>
{
    public GetPermissionPagedListQueryValidator()
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
/// Handler for permission paged list queries.
/// </summary>
public sealed class GetPermissionPagedListQueryHandler(IPermissionPagedListReadRepository readRepository)
    : GetPagedListQueryHandler<Permission, Guid, PermissionPagedListSpecification, PermissionPagedListFilter,
        GetPermissionPagedListQuery, PermissionPagedListResponseDto>(readRepository)
{
    protected override List<PermissionPagedListResponseDto> GetResultList(
        GetPermissionPagedListQuery request, IReadOnlyList<Permission> list)
    {
        return list.Select(p
            => new PermissionPagedListResponseDto(p.Id, p.Code, p.Name))
            .ToList();
    }
}
