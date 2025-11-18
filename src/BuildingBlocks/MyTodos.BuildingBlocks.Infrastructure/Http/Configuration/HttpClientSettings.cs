namespace MyTodos.BuildingBlocks.Infrastructure.Http.Configuration;

/// <summary>
/// Base configuration settings for HTTP clients.
/// </summary>
public class HttpClientSettings
{
    /// <summary>
    /// Gets or sets the base address for the HTTP client.
    /// </summary>
    public string BaseAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the request timeout in seconds. Default is 30 seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the maximum number of retries. Default is 3.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Gets or sets whether to use standard resilience policies.
    /// </summary>
    public bool UseResilience { get; set; } = true;
}
