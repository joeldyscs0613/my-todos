using System.Diagnostics.CodeAnalysis;

namespace MyTodos.SharedKernel.Helpers;

/// <summary>
/// Represents the result of an operation that can succeed or fail with error information.
/// Implements the Result pattern (railway-oriented programming).
/// </summary>
public class Result
{
    /// <summary>
    /// Initializes a new instance of the Result class with a list of errors.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation succeeded.</param>
    /// <param name="errors">The list of errors (empty if successful, non-empty if failed).</param>
    /// <exception cref="ArgumentException">Thrown when errors are inconsistent with success state.</exception>
    protected Result(bool isSuccess, List<Error> errors)
    {
        if (isSuccess && errors.Any() ||
            !isSuccess && errors.Any(e => e == Error.None))
        {
            throw new ArgumentException("Invalid error(s)", nameof(errors));
        }

        IsSuccess = isSuccess;
        Errors = errors.AsReadOnly(); // Make immutable

        ErrorType = Errors.Count switch
        {
            1 => Errors[0].Type,
            _ => ErrorType.BadRequest
        };
    }

    /// <summary>
    /// Initializes a new instance of the Result class with a single error.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation succeeded.</param>
    /// <param name="error">The error (if failed).</param>
    protected Result(bool isSuccess, Error error)
        : this(isSuccess, new List<Error> { error })
    {
    }

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the read-only list of errors. Empty if the operation succeeded.
    /// </summary>
    public IReadOnlyList<Error> Errors { get; }

    /// <summary>
    /// Gets the error type. For single errors, returns that error's type; otherwise returns BadRequest.
    /// </summary>
    public ErrorType ErrorType { get; }

    /// <summary>
    /// Gets the error descriptions as a read-only list of strings.
    /// </summary>
    public IReadOnlyList<string> ErrorDescriptions => Errors.Select(e => e.Description).ToList();

    /// <summary>
    /// Gets the first error, or Error.None if no errors exist.
    /// </summary>
    public Error FirstError => Errors.FirstOrDefault() ?? Error.None;

    /// <summary>
    /// Creates a successful result with no value.
    /// </summary>
    /// <returns>A successful Result instance.</returns>
    public static Result Success() => new(true, new List<Error>());

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="value">The value to wrap in the result.</param>
    /// <returns>A successful Result instance containing the value.</returns>
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, new List<Error>());

    /// <summary>
    /// Creates a failed result with a single error.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <returns>A failed Result instance.</returns>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Creates a failed result with multiple errors.
    /// </summary>
    /// <param name="errors">The list of errors.</param>
    /// <returns>A failed Result instance.</returns>
    public static Result Failure(List<Error> errors) => new(false, errors);

    /// <summary>
    /// Creates a failed result with a single error and a type parameter.
    /// </summary>
    /// <typeparam name="TValue">The type of the value (not used in failure).</typeparam>
    /// <param name="error">The error.</param>
    /// <returns>A failed Result instance.</returns>
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);

    /// <summary>
    /// Creates a failed result with multiple errors and a type parameter.
    /// </summary>
    /// <typeparam name="TValue">The type of the value (not used in failure).</typeparam>
    /// <param name="errors">The list of errors.</param>
    /// <returns>A failed Result instance.</returns>
    public static Result<TValue> Failure<TValue>(List<Error> errors) => new(default, false, errors);

    /// <summary>
    /// Creates a failed result indicating a null value error.
    /// </summary>
    /// <returns>A failed Result instance with NullValue error.</returns>
    public static Result NullValue() => new(false, Error.NullValue);

    #region Conflict

    /// <summary>
    /// Creates a failed result indicating a conflict error.
    /// </summary>
    /// <param name="description">The error description. Defaults to "The entity already exists."</param>
    /// <returns>A failed Result instance with a Conflict error.</returns>
    public static Result Conflict(string description = "The entity already exists.")
        => Failure(Error.Conflict(description));

    /// <summary>
    /// Creates a failed result indicating a conflict error with a type parameter.
    /// </summary>
    /// <typeparam name="TValue">The type of the value (not used in failure).</typeparam>
    /// <param name="description">The error description. Defaults to "The entity already exists."</param>
    /// <returns>A failed Result instance with a Conflict error.</returns>
    public static Result<TValue> Conflict<TValue>(string description = "The entity already exists.")
        => Failure<TValue>(Error.Conflict(description));

    #endregion

    #region Not Found

    /// <summary>
    /// Creates a failed result indicating a not found error.
    /// </summary>
    /// <param name="description">The error description. Defaults to "Not found."</param>
    /// <returns>A failed Result instance with a NotFound error.</returns>
    public static Result NotFound(string description = "Not found.")
        => Failure(Error.NotFound(description));

    /// <summary>
    /// Creates a failed result indicating a not found error with a type parameter.
    /// </summary>
    /// <typeparam name="TValue">The type of the value (not used in failure).</typeparam>
    /// <param name="description">The error description. Defaults to "Not found."</param>
    /// <returns>A failed Result instance with a NotFound error.</returns>
    public static Result<TValue> NotFound<TValue>(string description = "Not found.")
        => Failure<TValue>(Error.NotFound(description));

    #endregion

    #region Unauthorized

    /// <summary>
    /// Creates a failed result indicating an unauthorized error.
    /// </summary>
    /// <param name="description">The error description. Defaults to "Unauthorized."</param>
    /// <returns>A failed Result instance with an Unauthorized error.</returns>
    public static Result Unauthorized(string description = "Unauthorized.")
        => Failure(Error.Unauthorized(description));

    /// <summary>
    /// Creates a failed result indicating an unauthorized error with a type parameter.
    /// </summary>
    /// <typeparam name="TValue">The type of the value (not used in failure).</typeparam>
    /// <param name="description">The error description. Defaults to "Unauthorized."</param>
    /// <returns>A failed Result instance with an Unauthorized error.</returns>
    public static Result<TValue> Unauthorized<TValue>(string description = "Unauthorized.")
        => Failure<TValue>(Error.Unauthorized(description));

    #endregion

    #region Forbidden

    /// <summary>
    /// Creates a failed result indicating a forbidden error.
    /// </summary>
    /// <param name="description">The error description. Defaults to "Forbidden."</param>
    /// <returns>A failed Result instance with a Forbidden error.</returns>
    public static Result Forbidden(string description = "Forbidden.")
        => Failure(Error.Forbidden(description));

    /// <summary>
    /// Creates a failed result indicating a forbidden error with a type parameter.
    /// </summary>
    /// <typeparam name="TValue">The type of the value (not used in failure).</typeparam>
    /// <param name="description">The error description. Defaults to "Forbidden."</param>
    /// <returns>A failed Result instance with a Forbidden error.</returns>
    public static Result<TValue> Forbidden<TValue>(string description = "Forbidden.")
        => Failure<TValue>(Error.Forbidden(description));

    #endregion

    #region BadRequest

    /// <summary>
    /// Creates a failed result indicating a bad request error.
    /// </summary>
    /// <param name="description">The error description.</param>
    /// <returns>A failed Result instance with a BadRequest error.</returns>
    public static Result BadRequest(string description)
        => Failure(Error.BadRequest(description));

    /// <summary>
    /// Creates a failed result indicating a bad request error with a type parameter.
    /// </summary>
    /// <typeparam name="TValue">The type of the value (not used in failure).</typeparam>
    /// <param name="description">The error description.</param>
    /// <returns>A failed Result instance with a BadRequest error.</returns>
    public static Result<TValue> BadRequest<TValue>(string description)
        => Failure<TValue>(Error.BadRequest(description));

    /// <summary>
    /// Creates a failed result with multiple bad request errors.
    /// </summary>
    /// <param name="errors">The list of errors to convert to BadRequest type.</param>
    /// <returns>A failed Result instance with multiple BadRequest errors.</returns>
    public static Result BadRequest(List<Error> errors)
    {
        var badRequestErrors = errors
            .Select(e => new Error(ErrorType.BadRequest, e.Description))
            .ToList();
        return Failure(badRequestErrors);
    }

    /// <summary>
    /// Creates a failed result with multiple bad request errors from descriptions.
    /// </summary>
    /// <typeparam name="TValue">The type of the value (not used in failure).</typeparam>
    /// <param name="descriptions">The list of error descriptions.</param>
    /// <returns>A failed Result instance with multiple BadRequest errors.</returns>
    public static Result<TValue> BadRequest<TValue>(List<string> descriptions)
    {
        var badRequestErrors = descriptions
            .Select(d => new Error(ErrorType.BadRequest, d))
            .ToList();
        return Failure<TValue>(badRequestErrors);
    }

    /// <summary>
    /// Creates a failed result with multiple bad request errors with a type parameter.
    /// </summary>
    /// <typeparam name="TValue">The type of the value (not used in failure).</typeparam>
    /// <param name="errors">The list of errors to convert to BadRequest type.</param>
    /// <returns>A failed Result instance with multiple BadRequest errors.</returns>
    public static Result<TValue> BadRequest<TValue>(List<Error> errors)
    {
        var badRequestErrors = errors
            .Select(e => new Error(ErrorType.BadRequest, e.Description))
            .ToList();
        return Failure<TValue>(badRequestErrors);
    }

    #endregion

    /// <summary>
    /// Executes one of two actions based on whether the result is successful or failed.
    /// </summary>
    /// <param name="onSuccess">Action to execute if the result is successful.</param>
    /// <param name="onFailure">Action to execute if the result failed, receiving the list of errors.</param>
    public void Match(Action onSuccess, Action<List<Error>> onFailure)
    {
        if (IsSuccess)
            onSuccess();
        else
            onFailure(Errors.ToList());
    }

    /// <summary>
    /// Creates a result from a nullable value. Returns success if not null, failure with NullValue error if null.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <returns>A successful Result if value is not null; otherwise a failed Result with NullValue error.</returns>
    public static Result<TValue> Create<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);
}

/// <summary>
/// Represents the result of an operation that can succeed with a value or fail with error information.
/// </summary>
/// <typeparam name="TValue">The type of the value returned on success.</typeparam>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    /// <summary>
    /// Initializes a new instance of the Result{TValue} class with a single error.
    /// </summary>
    /// <param name="value">The value (if successful).</param>
    /// <param name="isSuccess">Indicates whether the operation succeeded.</param>
    /// <param name="error">The error (if failed).</param>
    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Initializes a new instance of the Result{TValue} class with multiple errors.
    /// </summary>
    /// <param name="value">The value (if successful).</param>
    /// <param name="isSuccess">Indicates whether the operation succeeded.</param>
    /// <param name="errors">The list of errors (if failed).</param>
    protected internal Result(TValue? value, bool isSuccess, List<Error> errors)
        : base(isSuccess, errors)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the value if the result is successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing the value of a failed result.</exception>
    [NotNull]
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can't be accessed.");

    /// <summary>
    /// Executes one of two functions based on whether the result is successful or failed, and returns a value.
    /// </summary>
    /// <typeparam name="TResult">The type of the result to return.</typeparam>
    /// <param name="onSuccess">Function to execute if the result is successful, receiving the value.</param>
    /// <param name="onFailure">Function to execute if the result failed, receiving the list of errors.</param>
    /// <returns>The result of the executed function.</returns>
    public TResult Match<TResult>(Func<TValue, TResult> onSuccess, Func<List<Error>, TResult> onFailure)
        => IsSuccess ? onSuccess(Value) : onFailure(Errors.ToList());

    /// <summary>
    /// Executes one of two actions based on whether the result is successful or failed.
    /// </summary>
    /// <param name="onSuccess">Action to execute if the result is successful, receiving the value.</param>
    /// <param name="onFailure">Action to execute if the result failed, receiving the list of errors.</param>
    public void Match(Action<TValue> onSuccess, Action<List<Error>> onFailure)
    {
        if (IsSuccess)
            onSuccess(Value);
        else
            onFailure(Errors.ToList());
    }

    /// <summary>
    /// Implicitly converts a value to a Result. Returns success if not null, failure with NullValue error if null.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);
}