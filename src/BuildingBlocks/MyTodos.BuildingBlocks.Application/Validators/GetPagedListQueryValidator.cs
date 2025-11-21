using FluentValidation;
using MyTodos.BuildingBlocks.Application.Abstractions.Filters;
using MyTodos.BuildingBlocks.Application.Abstractions.Queries;
using MyTodos.BuildingBlocks.Application.Constants;

namespace MyTodos.BuildingBlocks.Application.Validators;

/// <summary>
/// Base validator for paged list queries with common pagination and sorting validation rules.
/// </summary>
/// <typeparam name="TQuery">The type of paged list query being validated.</typeparam>
/// <typeparam name="TSpecification">The specification type used by the query.</typeparam>
/// <typeparam name="TFilter">The filter type used by the query.</typeparam>
/// <typeparam name="TResponseItemDto">The response item DTO type.</typeparam>
public abstract class PagedListQueryValidator<TQuery, TSpecification, TFilter, TResponseItemDto>
    : QueryValidator<TQuery>
    where TQuery : PagedListQuery<TSpecification, TFilter, TResponseItemDto>
    where TFilter : Filter
    where TSpecification : class
{
    /// <summary>
    /// Initializes a new instance of the PagedListQueryValidator class with common validation rules.
    /// </summary>
    protected PagedListQueryValidator()
    {
        // Validate Filter object is not null
        RuleFor(x => x.Filter)
            .NotNull()
            .WithMessage("Filter cannot be null.");

        // Validate PageNumber (only when Filter is not null)
        RuleFor(x => x.Filter.PageNumber)
            .GreaterThanOrEqualTo(PageListConstants.DefaultPageNumber)
            .When(x => x.Filter != null)
            .WithMessage($"Page number must be at least {PageListConstants.DefaultPageNumber}.");

        // Validate PageSize (only when Filter is not null)
        RuleFor(x => x.Filter.PageSize)
            .GreaterThanOrEqualTo(PageListConstants.MinPageSize)
            .When(x => x.Filter != null)
            .WithMessage($"Page size must be at least {PageListConstants.MinPageSize}.")
            .LessThanOrEqualTo(GetMaxPageSize())
            .When(x => x.Filter != null)
            .WithMessage($"Page size must not exceed {GetMaxPageSize()}.");

        // Validate SortDirection (only when Filter is not null)
        RuleFor(x => x.Filter.SortDirection)
            .Must(BeValidSortDirection)
            .When(x => x.Filter != null && !string.IsNullOrWhiteSpace(x.Filter.SortDirection))
            .WithMessage("Sort direction must be either 'asc' or 'desc'.");

        // Validate SearchBy max length (only when Filter is not null)
        RuleFor(x => x.Filter.SearchBy)
            .MaximumLength(GetMaxSearchLength())
            .When(x => x.Filter != null && !string.IsNullOrWhiteSpace(x.Filter.SearchBy))
            .WithMessage($"Search term must not exceed {GetMaxSearchLength()} characters.");
    }

    /// <summary>
    /// Gets the maximum allowed page size. Can be overridden for export scenarios.
    /// </summary>
    /// <returns>The maximum page size (default: 50).</returns>
    protected virtual int GetMaxPageSize() => PageListConstants.MaxPageSize;

    /// <summary>
    /// Gets the maximum allowed search term length. Can be overridden per query type.
    /// </summary>
    /// <returns>The maximum search length (default: 200).</returns>
    protected virtual int GetMaxSearchLength() => 200;

    /// <summary>
    /// Validates that a sort direction value is either "asc" or "desc" (case-insensitive).
    /// </summary>
    /// <param name="sortDirection">The sort direction to validate.</param>
    /// <returns>True if valid, false otherwise.</returns>
    protected static bool BeValidSortDirection(string? sortDirection)
    {
        if (string.IsNullOrWhiteSpace(sortDirection))
            return true; // Allow empty/null (will use default)

        var normalized = sortDirection.Trim().ToLowerInvariant();
        return normalized == "asc" || normalized == "desc";
    }

    /// <summary>
    /// Validates that a sort field is in the list of allowed sort fields.
    /// </summary>
    /// <param name="sortField">The sort field to validate.</param>
    /// <param name="validSortFields">The list of valid sort field names.</param>
    /// <returns>True if valid, false otherwise.</returns>
    protected static bool BeValidSortField(string? sortField, IReadOnlyList<string> validSortFields)
    {
        if (string.IsNullOrWhiteSpace(sortField))
            return true; // Allow empty/null (will use default)

        if (validSortFields == null || validSortFields.Count == 0)
            return true; // No restrictions if list is empty

        return validSortFields.Any(valid =>
            string.Equals(valid, sortField?.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Creates an error message for an invalid sort field.
    /// </summary>
    /// <param name="sortField">The invalid sort field.</param>
    /// <param name="validSortFields">The list of valid sort field names.</param>
    /// <returns>A descriptive error message.</returns>
    protected static string GetInvalidSortFieldMessage(string sortField, IReadOnlyList<string> validSortFields)
    {
        var validFieldsList = string.Join(", ", validSortFields);
        return $"'{sortField}' is not a valid sort field. Valid fields are: {validFieldsList}.";
    }
}
