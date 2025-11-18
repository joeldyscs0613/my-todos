namespace MyTodos.BuildingBlocks.Infrastructure.Http.Exceptions;

/// <summary>
/// Exception thrown when a circuit breaker is open and requests are being rejected.
/// </summary>
public class CircuitBreakerOpenException : ServiceUnavailableException
{
    public CircuitBreakerOpenException(string serviceName)
        : base($"Circuit breaker is open for service: {serviceName}." +
               $" Requests are being rejected to allow the service to recover.")
    {
    }

    public CircuitBreakerOpenException(string serviceName, string requestUri)
        : base($"Circuit breaker is open for service: {serviceName}. " +
               $"Requests are being rejected to allow the service to recover.", requestUri)
    {
    }
}
