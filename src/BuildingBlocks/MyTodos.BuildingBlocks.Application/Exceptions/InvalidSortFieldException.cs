using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.BuildingBlocks.Application.Exceptions;

/// <summary>
/// Exception thrown when an invalid sort field is provided to a specification.
/// This represents a validation error for user-provided sort field names that don't match
/// the specification's defined sortable fields.
/// </summary>
/// <remarks>
/// <para><strong>Usage Context:</strong></para>
/// <para>
/// This exception is thrown during specification application when the provided sort field
/// doesn't exist in the specification's GetSortFunctions() dictionary. It serves as a safety
/// net for invalid user input that wasn't caught by earlier validation layers.
/// </para>
/// <para><strong>Error Handling:</strong></para>
/// <para>
/// The global exception handler maps this to a 400 Bad Request response. The exception message
/// (including the invalid field and list of valid fields) is safe to expose to end users in
/// production since it's developer-controlled and helps users correct their requests.
/// </para>
/// <para><strong>Design Decision:</strong></para>
/// <para>
/// This inherits from DomainException (not ArgumentException) because determining which fields
/// are sortable is domain knowledge encapsulated in specifications. The valid sort fields
/// represent domain concepts (e.g., "DueDate", "Priority", "Title") rather than technical
/// implementation details.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // In Specification.ApplySort():
/// if (!sortFunctions.TryGetValue(sortField, out var sortExpression))
/// {
///     throw new InvalidSortFieldException(
///         providedField: "invalidField",
///         validFields: new[] { "Title", "DueDate", "Priority" });
/// }
///
/// // Results in 400 Bad Request with message:
/// // "Sort field 'invalidField' is not valid. Valid fields: Title, DueDate, Priority"
/// </code>
/// </example>
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
