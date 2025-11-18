using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace MyTodos.BuildingBlocks.Infrastructure.Http.Handlers;

/// <summary>
/// Delegating handler that logs HTTP requests and responses with timing information.
/// </summary>
public sealed class LoggingDelegatingHandler : DelegatingHandler
{
    private readonly ILogger<LoggingDelegatingHandler> _logger;

    public LoggingDelegatingHandler(ILogger<LoggingDelegatingHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Sending {Method} request to {RequestUri}",
            request.Method,
            request.RequestUri);

        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            stopwatch.Stop();

            var logLevel = response.IsSuccessStatusCode ? LogLevel.Information : LogLevel.Warning;

            _logger.Log(
                logLevel,
                "Received {StatusCode} response from {RequestUri} in {ElapsedMilliseconds}ms",
                (int)response.StatusCode,
                request.RequestUri,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Request to {RequestUri} failed after {ElapsedMilliseconds}ms",
                request.RequestUri,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
