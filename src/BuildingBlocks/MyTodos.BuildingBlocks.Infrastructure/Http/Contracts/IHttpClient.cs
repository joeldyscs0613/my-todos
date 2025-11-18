namespace MyTodos.BuildingBlocks.Infrastructure.Http.Contracts;

/// <summary>
/// Defines a contract for HTTP client operations with built-in error handling.
/// </summary>
public interface IHttpClient
{
    /// <summary>
    /// Sends a GET request to the specified URI.
    /// </summary>
    Task<TResponse?> GetAsync<TResponse>(
        string uri,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a POST request to the specified URI.
    /// </summary>
    Task<TResponse?> PostAsync<TRequest, TResponse>(
        string uri,
        TRequest content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a PUT request to the specified URI.
    /// </summary>
    Task<TResponse?> PutAsync<TRequest, TResponse>(
        string uri,
        TRequest content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a DELETE request to the specified URI.
    /// </summary>
    Task DeleteAsync(
        string uri,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a PATCH request to the specified URI.
    /// </summary>
    Task<TResponse?> PatchAsync<TRequest, TResponse>(
        string uri,
        TRequest content,
        CancellationToken cancellationToken = default);
}
