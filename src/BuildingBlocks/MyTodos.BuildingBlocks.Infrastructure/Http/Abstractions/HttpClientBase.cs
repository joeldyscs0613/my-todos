using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MyTodos.BuildingBlocks.Infrastructure.Http.Constants;
using MyTodos.BuildingBlocks.Infrastructure.Http.Contracts;
using MyTodos.BuildingBlocks.Infrastructure.Http.Exceptions;

namespace MyTodos.BuildingBlocks.Infrastructure.Http.Abstractions;

/// <summary>
/// Base class for HTTP client implementations providing common HTTP operation patterns.
/// Handles serialization, error handling, and logging.
/// </summary>
public abstract class HttpClientBase : IHttpClient
{
    protected readonly HttpClient HttpClient;
    protected readonly ILogger Logger;
    protected readonly JsonSerializerOptions JsonOptions;

    protected HttpClientBase(HttpClient httpClient, ILogger logger)
    {
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));

        JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public virtual async Task<TResponse?> GetAsync<TResponse>(
        string uri,
        CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Sending GET request to {Uri}", uri);

        var response = await HttpClient.GetAsync(uri, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);
    }

    public virtual async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string uri,
        TRequest content,
        CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Sending POST request to {Uri}", uri);

        var response = await HttpClient.PostAsJsonAsync(uri, content, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);
    }

    public virtual async Task<TResponse?> PutAsync<TRequest, TResponse>(
        string uri,
        TRequest content,
        CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Sending PUT request to {Uri}", uri);

        var response = await HttpClient.PutAsJsonAsync(uri, content, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);
    }

    public virtual async Task<TResponse?> PatchAsync<TRequest, TResponse>(
        string uri,
        TRequest content,
        CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Sending PATCH request to {Uri}", uri);

        var jsonContent = JsonSerializer.Serialize(content, JsonOptions);
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, MediaTypeConstants.ApplicationJson);

        var response = await HttpClient.PatchAsync(uri, httpContent, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);
    }

    public virtual async Task DeleteAsync(
        string uri,
        CancellationToken cancellationToken = default)
    {
        Logger.LogDebug("Sending DELETE request to {Uri}", uri);

        var response = await HttpClient.DeleteAsync(uri, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
