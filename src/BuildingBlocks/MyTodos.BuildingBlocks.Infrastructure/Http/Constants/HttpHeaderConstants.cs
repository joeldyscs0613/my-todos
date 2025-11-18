namespace MyTodos.BuildingBlocks.Infrastructure.Http.Constants;

/// <summary>
/// Constants for common HTTP headers.
/// </summary>
public static class HttpHeaderConstants
{
    /// <summary>
    /// Correlation ID header for distributed tracing.
    /// </summary>
    public const string CorrelationId = "X-Correlation-ID";

    /// <summary>
    /// Request ID header.
    /// </summary>
    public const string RequestId = "X-Request-ID";

    /// <summary>
    /// Content-Type header.
    /// </summary>
    public const string ContentType = "Content-Type";

    /// <summary>
    /// Accept header.
    /// </summary>
    public const string Accept = "Accept";

    /// <summary>
    /// Authorization header.
    /// </summary>
    public const string Authorization = "Authorization";

    /// <summary>
    /// User-Agent header.
    /// </summary>
    public const string UserAgent = "User-Agent";
}
