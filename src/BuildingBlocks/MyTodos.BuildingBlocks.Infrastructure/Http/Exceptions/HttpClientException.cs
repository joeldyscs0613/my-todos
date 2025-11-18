using System.Net;

namespace MyTodos.BuildingBlocks.Infrastructure.Http.Exceptions;

/// <summary>
/// Base exception for HTTP client errors.
/// </summary>
public class HttpClientException : Exception
{
    /// <summary>
    /// Gets the HTTP status code associated with this exception.
    /// </summary>
    public HttpStatusCode? StatusCode { get; }

    /// <summary>
    /// Gets the request URI that caused the exception.
    /// </summary>
    public string? RequestUri { get; }

    public HttpClientException(string message)
        : base(message)
    {
    }

    public HttpClientException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public HttpClientException(string message, HttpStatusCode statusCode, string? requestUri = null)
        : base(message)
    {
        StatusCode = statusCode;
        RequestUri = requestUri;
    }

    public HttpClientException(
        string message,
        HttpStatusCode statusCode,
        string? requestUri,
        Exception innerException)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        RequestUri = requestUri;
    }
}
