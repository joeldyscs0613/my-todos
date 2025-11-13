using FluentValidation;
using MyTodos.BuildingBlocks.Application.Abstractions.Filters;
using MyTodos.BuildingBlocks.Application.Constants;

namespace MyTodos.BuildingBlocks.Application.Validators;

/// <summary>
/// Base validator for filters with common validation rules.
/// </summary>
/// <typeparam name="TFilter">The type of filter being validated.</typeparam>
public abstract class FilterValidator<TFilter> : AbstractValidator<TFilter>
    where TFilter : Filter
{
    protected FilterValidator()
    {
        // Validate PageNumber
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(PageListConstants.DefaultPageNumber)
            .WithMessage($"Page number must be at least {PageListConstants.DefaultPageNumber}.");

        // Validate PageSize
        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(PageListConstants.MinPageSize)
            .WithMessage($"Page size must be at least {PageListConstants.MinPageSize}.")
            .LessThanOrEqualTo(GetMaxPageSize())
            .WithMessage($"Page size must not exceed {GetMaxPageSize()}.");

        // Validate SortDirection
        RuleFor(x => x.SortDirection)
            .Must(BeValidSortDirection)
            .When(x => !string.IsNullOrWhiteSpace(x.SortDirection))
            .WithMessage("Sort direction must be either 'asc' or 'desc'.");

        // Validate SearchBy max length
        RuleFor(x => x.SearchBy)
            .MaximumLength(GetMaxSearchLength())
            .When(x => !string.IsNullOrWhiteSpace(x.SearchBy))
            .WithMessage($"Search term must not exceed {GetMaxSearchLength()} characters.");
    }

    /// <summary>
    /// Validates sort direction is "asc" or "desc" (case-insensitive).
    /// </summary>
    protected static bool BeValidSortDirection(string? sortDirection)
    {
        if (string.IsNullOrWhiteSpace(sortDirection))
            return true; // Allow empty/null (will use default)

        var normalized = sortDirection.Trim().ToLowerInvariant();
        return normalized == "asc" || normalized == "desc";
    }

    /// <summary>
    /// Gets the maximum allowed page size. Override for export scenarios.
    /// </summary>
    protected virtual int GetMaxPageSize() => PageListConstants.MaxPageSize;

    /// <summary>
    /// Gets the maximum allowed search term length. Override per filter type.
    /// </summary>
    protected virtual int GetMaxSearchLength() => 200;
}
