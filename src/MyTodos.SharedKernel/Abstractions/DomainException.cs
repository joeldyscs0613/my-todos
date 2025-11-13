namespace MyTodos.SharedKernel.Abstractions;

/// <summary>
/// Base exception class for domain-layer exceptions that represent invariant violations.
/// Messages from DomainException are considered safe to expose to end users.
/// Use this sparingly - only for true invariant violations and "should never happen" scenarios.
/// For expected business rule failures, prefer using the Result pattern instead.
/// </summary>
/// <remarks>
/// <para><strong>When to use DomainException:</strong></para>
/// <list type="bullet">
/// <item><description>Entity factory guard clauses (e.g., required parameters)</description></item>
/// <item><description>Invariant protection (e.g., state machine violations)</description></item>
/// <item><description>Programming errors that should be caught during development</description></item>
/// </list>
/// <para><strong>When to use Result pattern instead:</strong></para>
/// <list type="bullet">
/// <item><description>Expected business rule validations</description></item>
/// <item><description>Flow control scenarios</description></item>
/// <item><description>User input validation</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Good use of DomainException - invariant violation
/// public static Task Create(string title, ProjectId projectId)
/// {
///     if (string.IsNullOrWhiteSpace(title))
///         throw new DomainException("Task title cannot be empty");
///
///     return new Task(title, projectId);
/// }
///
/// // Prefer Result pattern for business rules
/// public Result AssignTo(UserId userId)
/// {
///     if (Status == TaskStatus.Completed)
///         return Result.Failure("Cannot reassign a completed task");
///
///     AssignedTo = userId;
///     return Result.Success();
/// }
/// </code>
/// </example>
public class DomainException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error. This message will be shown to end users in production.</param>
    public DomainException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error. This message will be shown to end users in production.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
