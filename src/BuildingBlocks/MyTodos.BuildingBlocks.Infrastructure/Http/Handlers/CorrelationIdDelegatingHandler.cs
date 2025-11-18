using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MyTodos.BuildingBlocks.Infrastructure.Http.Constants;

namespace MyTodos.BuildingBlocks.Infrastructure.Http.Handlers;

/// <summary>
/// Delegating handler that propagates correlation IDs across HTTP requests for distributed tracing.
/// Creates a new correlation ID if one doesn't exist in the current context.
/// </summary>
public sealed class CorrelationIdDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor? _httpContextAccessor;
    private readonly ILogger<CorrelationIdDelegatingHandler> _logger;

    public CorrelationIdDelegatingHandler(
        ILogger<CorrelationIdDelegatingHandler> logger,
        IHttpContextAccessor? httpContextAccessor = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var correlationId = GetOrCreateCorrelationId();

        if (!request.Headers.Contains(HttpHeaderConstants.CorrelationId))
        {
            request.Headers.Add(HttpHeaderConstants.CorrelationId, correlationId);
            _logger.LogDebug("Added correlation ID {CorrelationId} to outgoing request to {RequestUri}",
                correlationId, request.RequestUri);
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private string GetOrCreateCorrelationId()
    {
        // Try to get correlation ID from current HTTP context
        if (_httpContextAccessor?.HttpContext?.Request.Headers.TryGetValue(
                HttpHeaderConstants.CorrelationId, out var correlationId) == true)
        {
            return correlationId.ToString();
        }

        // Generate new correlation ID if not in context
        var newCorrelationId = Guid.NewGuid().ToString();
        _logger.LogDebug("Generated new correlation ID: {CorrelationId}", newCorrelationId);

        return newCorrelationId;
    }
}
