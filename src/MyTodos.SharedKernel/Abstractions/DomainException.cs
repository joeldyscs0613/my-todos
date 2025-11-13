using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

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

    /// <summary>
    /// Throws a <see cref="DomainException"/> if the specified string value is null, empty, or consists only of white-space characters.
    /// </summary>
    /// <param name="value">The string value to validate.</param>
    /// <param name="message">The error message to use if validation fails. This message will be shown to end users in production.</param>
    /// <param name="paramName">The name of the parameter being validated (automatically captured).</param>
    /// <exception cref="DomainException">Thrown when <paramref name="value"/> is null, empty, or whitespace.</exception>
    /// <example>
    /// <code>
    /// public void SetUsername(string username)
    /// {
    ///     DomainException.ThrowIfNullOrWhiteSpace(username, "Username cannot be empty.");
    ///     _username = username;
    /// }
    /// </code>
    /// </example>
    public static void ThrowIfNullOrWhiteSpace(
        [NotNull] string? value,
        string message,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException(paramName != null ? $"{message} (Parameter: {paramName})" : message);
        }
    }

    /// <summary>
    /// Throws a <see cref="DomainException"/> if the specified string value is null or empty.
    /// </summary>
    /// <param name="value">The string value to validate.</param>
    /// <param name="message">The error message to use if validation fails. This message will be shown to end users in production.</param>
    /// <param name="paramName">The name of the parameter being validated (automatically captured).</param>
    /// <exception cref="DomainException">Thrown when <paramref name="value"/> is null or empty.</exception>
    /// <example>
    /// <code>
    /// public void SetEventType(string eventType)
    /// {
    ///     DomainException.ThrowIfNullOrEmpty(eventType, "Event type cannot be empty.");
    ///     _eventType = eventType;
    /// }
    /// </code>
    /// </example>
    public static void ThrowIfNullOrEmpty(
        [NotNull] string? value,
        string message,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new DomainException(paramName != null ? $"{message} (Parameter: {paramName})" : message);
        }
    }

    /// <summary>
    /// Throws a <see cref="DomainException"/> if the specified value is null.
    /// </summary>
    /// <typeparam name="T">The type of the value to validate.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="message">The error message to use if validation fails. This message will be shown to end users in production.</param>
    /// <param name="paramName">The name of the parameter being validated (automatically captured).</param>
    /// <exception cref="DomainException">Thrown when <paramref name="value"/> is null.</exception>
    /// <example>
    /// <code>
    /// public void SetCategory(Category category)
    /// {
    ///     DomainException.ThrowIfNull(category, "Category cannot be null.");
    ///     _category = category;
    /// }
    /// </code>
    /// </example>
    public static void ThrowIfNull<T>(
        [NotNull] T? value,
        string message,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value is null)
        {
            throw new DomainException(paramName != null ? $"{message} (Parameter: {paramName})" : message);
        }
    }
}
