using System.Diagnostics;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.BuildingBlocks.Application.Behaviors;

/// <summary>
/// Logs request execution details including timing, success/failure status, and errors.
/// Provides observability for all MediatR requests in the application pipeline.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Handles the request by logging execution details.
    /// Logs request start, measures execution time, and logs outcome (success, failure, or exception).
    /// </summary>
    /// <param name="request">The request being processed.</param>
    /// <param name="next">The next handler in the pipeline.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The response from the handler.</returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct = default)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        // Log request start with detailed payload for debugging
        logger.LogDebug(
            "Handling {RequestName} with payload: {RequestPayload}",
            requestName,
            JsonSerializer.Serialize(request));

        try
        {
            // Execute the handler
            var response = await next(ct);
            stopwatch.Stop();

            // Check if response follows Result pattern and log accordingly
            if (response is Result result)
            {
                if (result.IsFailure)
                {
                    // Log business logic failures (not exceptions)
                    logger.LogWarning(
                        "Request {RequestName} completed with failure in {ElapsedMilliseconds}ms. Errors: {Errors}",
                        requestName,
                        stopwatch.ElapsedMilliseconds,
                        JsonSerializer.Serialize(result.ErrorDescriptions));
                }
                else
                {
                    // Log successful execution
                    logger.LogInformation(
                        "Request {RequestName} completed successfully in {ElapsedMilliseconds}ms",
                        requestName,
                        stopwatch.ElapsedMilliseconds);
                }
            }
            else
            {
                // Non-Result responses - assume successful
                logger.LogInformation(
                    "Request {RequestName} completed in {ElapsedMilliseconds}ms",
                    requestName,
                    stopwatch.ElapsedMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Log exception - will be caught by global exception handler for API response
            logger.LogError(
                ex,
                "Request {RequestName} failed with exception after {ElapsedMilliseconds}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);

            // Re-throw to maintain pipeline behavior and allow global handler to catch
            throw;
        }
    }
}
