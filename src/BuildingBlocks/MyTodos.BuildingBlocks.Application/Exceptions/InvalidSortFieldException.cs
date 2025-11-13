using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.BuildingBlocks.Application.Exceptions;

/// <summary>
/// Exception thrown when an invalid sort field is provided to a specification.
/// Mapped to 400 Bad Request by global exception handler.
/// </summary>
public class InvalidSortFieldException : DomainException
{
    /// <summary>
    /// Gets the sort field name that was provided by the user.
    /// </summary>
    public string ProvidedField { get; }

    /// <summary>
    /// Gets the list of valid sort field names for the specification.
    /// </summary>
    public IReadOnlyList<string> ValidFields { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidSortFieldException"/> class.
    /// </summary>
    /// <param name="providedField">The invalid sort field name that was provided.</param>
    /// <param name="validFields">The collection of valid sort field names.</param>
    public InvalidSortFieldException(string providedField, IEnumerable<string> validFields)
        : base(FormatMessage(providedField, validFields))
    {
        ProvidedField = providedField;
        ValidFields = validFields.ToList().AsReadOnly();
    }

    private static string FormatMessage(string providedField, IEnumerable<string> validFields)
    {
        var validFieldsList = string.Join(", ", validFields.OrderBy(f => f));
        return $"Sort field '{providedField}' is not valid. Valid fields: {validFieldsList}";
    }
}
