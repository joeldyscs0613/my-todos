using FluentValidation;
using MyTodos.BuildingBlocks.Application.Validators;

namespace MyTodos.Services.IdentityService.Application.Users.Queries.GetPagedList;

public sealed class UserPagedListQueryValidator 
    : PagedListQueryValidator<UserPagedListQuery, UserPagedListSpecification,
        UserPagedListFilter, UserPagedListResponseDto>
{
    public UserPagedListQueryValidator()
    {
        // Add sort field validation
        RuleFor(x => x.Filter.SortField)
            .Must((query, sortField) => BeValidSortField(sortField, query.Specification.ValidSortFields))
            .When(x => x.Filter != null && !string.IsNullOrWhiteSpace(x.Filter.SortField))
            .WithMessage((query, sortField) =>
                GetInvalidSortFieldMessage(sortField!, query.Specification.ValidSortFields));
    }
}