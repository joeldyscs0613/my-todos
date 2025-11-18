using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyTodos.BuildingBlocks.Infrastructure.Http.Abstractions;
using MyTodos.BuildingBlocks.Infrastructure.Http.Configuration;
using MyTodos.BuildingBlocks.Infrastructure.Http.Contracts;

namespace MyTodos.BuildingBlocks.Infrastructure.Http.ServiceClients;

/// <summary>
/// Base class for service-to-service HTTP clients.
/// Provides common functionality for microservice communication.
/// </summary>
public abstract class ServiceClientBase : HttpClientBase, ITypedHttpClient
{
    protected ServiceClientBase(
        HttpClient httpClient,
        ILogger logger)
        : base(httpClient, logger)
    {
        BaseAddress = httpClient.BaseAddress?.ToString() ?? string.Empty;
    }

    protected ServiceClientBase(
        HttpClient httpClient,
        ILogger logger,
        IOptions<HttpClientSettings> settings)
        : base(httpClient, logger)
    {
        var clientSettings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));

        if (!string.IsNullOrWhiteSpace(clientSettings.BaseAddress))
        {
            httpClient.BaseAddress = new Uri(clientSettings.BaseAddress);
        }

        httpClient.Timeout = TimeSpan.FromSeconds(clientSettings.TimeoutSeconds);
        BaseAddress = httpClient.BaseAddress?.ToString() ?? clientSettings.BaseAddress;
    }

    /// <summary>
    /// Gets the base address for this service client.
    /// </summary>
    public string BaseAddress { get; }

    /// <summary>
    /// Builds a complete URI by combining the base address with a relative path.
    /// </summary>
    protected string BuildUri(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return BaseAddress;
        }

        // Remove leading slash from relative path if present
        if (relativePath.StartsWith('/'))
        {
            relativePath = relativePath[1..];
        }

        // Ensure base address ends with slash
        var baseUri = BaseAddress.EndsWith('/') ? BaseAddress : $"{BaseAddress}/";

        return $"{baseUri}{relativePath}";
    }
}
