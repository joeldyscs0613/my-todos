using System.Net;

namespace MyTodos.BuildingBlocks.Infrastructure.Http.Exceptions;

/// <summary>
/// Exception thrown when a service is unavailable (503) or the circuit breaker is open.
/// </summary>
public class ServiceUnavailableException : HttpClientException
{
    public ServiceUnavailableException(string message)
        : base(message, HttpStatusCode.ServiceUnavailable)
    {
    }

    public ServiceUnavailableException(string message, string requestUri)
        : base(message, HttpStatusCode.ServiceUnavailable, requestUri)
    {
    }

    public ServiceUnavailableException(string message, Exception innerException)
        : base(message, HttpStatusCode.ServiceUnavailable, null, innerException)
    {
    }
}
