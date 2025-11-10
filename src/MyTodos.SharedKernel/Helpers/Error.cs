namespace MyTodos.SharedKernel.Helpers;

/// <summary>
/// Represents an error with a type and description, used in the Result pattern.
/// </summary>
/// <param name="Type">The type/category of the error.</param>
/// <param name="Description">A human-readable description of the error.</param>
public record Error(ErrorType Type, string Description)
{
    /// <summary>
    /// Represents the absence of an error (used internally).
    /// </summary>
    public static readonly Error None = new(ErrorType.InternalServerError, string.Empty);

    /// <summary>
    /// Represents an error when a null value was provided where one was not expected.
    /// </summary>
    public static readonly Error NullValue = new(ErrorType.NullValue, "Null value was provided");

    /// <summary>
    /// Creates a NotFound error with an optional custom description.
    /// </summary>
    /// <param name="description">The error description. Defaults to "Not found."</param>
    /// <returns>An Error instance representing a NotFound error.</returns>
    public static Error NotFound(string description = "Not found.")
        => new(ErrorType.NotFound, description);

    /// <summary>
    /// Creates a Conflict error with an optional custom description.
    /// </summary>
    /// <param name="description">The error description. Defaults to "The entity already exists."</param>
    /// <returns>An Error instance representing a Conflict error.</returns>
    public static Error Conflict(string description = "The entity already exists.")
        => new(ErrorType.Conflict, description);

    /// <summary>
    /// Creates an Unauthorized error with an optional custom description.
    /// </summary>
    /// <param name="description">The error description. Defaults to "Unauthorized."</param>
    /// <returns>An Error instance representing an Unauthorized error.</returns>
    public static Error Unauthorized(string description = "Unauthorized.")
        => new(ErrorType.Unauthorized, description);

    /// <summary>
    /// Creates a Forbidden error with an optional custom description.
    /// </summary>
    /// <param name="description">The error description. Defaults to "Forbidden."</param>
    /// <returns>An Error instance representing a Forbidden error.</returns>
    public static Error Forbidden(string description = "Forbidden.")
        => new(ErrorType.Forbidden, description);

    /// <summary>
    /// Creates a BadRequest error with the specified description.
    /// </summary>
    /// <param name="description">The error description.</param>
    /// <returns>An Error instance representing a BadRequest error.</returns>
    public static Error BadRequest(string description)
        => new(ErrorType.BadRequest, description);
}

/// <summary>
/// Defines the types/categories of errors that can occur in the application.
/// Values correspond to HTTP status codes where applicable.
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// Internal server error (HTTP 500).
    /// </summary>
    InternalServerError = 500,

    /// <summary>
    /// Bad request - invalid input (HTTP 400).
    /// </summary>
    BadRequest = 400,

    /// <summary>
    /// Unauthorized - authentication required (HTTP 401).
    /// </summary>
    Unauthorized = 401,

    /// <summary>
    /// Forbidden - insufficient permissions (HTTP 403).
    /// </summary>
    Forbidden = 403,

    /// <summary>
    /// Not found - resource does not exist (HTTP 404).
    /// </summary>
    NotFound = 404,

    /// <summary>
    /// Conflict - resource already exists or version conflict (HTTP 409).
    /// </summary>
    Conflict = 409,

    /// <summary>
    /// Null value - unprocessable entity (HTTP 422).
    /// </summary>
    NullValue = 422
}