using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MyTodos.BuildingBlocks.Presentation.ProblemDetails;

namespace MyTodos.BuildingBlocks.Presentation.Middleware;

/// <summary>
/// Global exception handler that catches unhandled exceptions and converts them to RFC 7807 Problem Details responses.
/// Logs exceptions that occur outside the MediatR pipeline (pipeline exceptions are already logged by LoggingBehavior).
/// Uses the modern IExceptionHandler pattern introduced in .NET 7/8.
/// </summary>
public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    ProblemDetailsFactory problemDetailsFactory) : IExceptionHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Attempts to handle the exception by converting it to a Problem Details response.
    /// </summary>
    /// <param name="httpContext">The HTTP context for the current request.</param>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="ct">Cancellation token for the async operation.</param>
    /// <returns>True if the exception was handled; otherwise, false.</returns>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken ct)
    {
        // Log exception only if it occurred outside MediatR pipeline
        // MediatR exceptions are already logged by LoggingBehavior
        if (!IsMediatRException(exception))
        {
            logger.LogError(
                exception,
                "Unhandled exception occurred outside MediatR pipeline at {Path}",
                httpContext.Request.Path);
        }
        else
        {
            // MediatR exception - already logged, just converting to response
            logger.LogDebug(
                "Converting MediatR exception to Problem Details response for {Path}",
                httpContext.Request.Path);
        }

        // Convert exception to Problem Details and send response
        await HandleExceptionAsync(httpContext, exception, ct);

        // Return true to indicate the exception was handled
        return true;
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception,
        CancellationToken ct)
    {
        // Create Problem Details response from exception
        var problemDetails = problemDetailsFactory.CreateFromException(context, exception);

        // Set response status code and content type
        context.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/problem+json";

        try
        {
            // Serialize and write Problem Details to response
            await context.Response.WriteAsJsonAsync(problemDetails, JsonOptions, ct);
        }
        catch (Exception serializationException)
        {
            // If serialization fails, log and send a minimal error response
            logger.LogError(
                serializationException,
                "Failed to serialize Problem Details response for exception: {OriginalException}",
                exception.Message);

            // Fallback to plain text response
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync("An error occurred while processing your request.", ct);
        }
    }

    private static bool IsMediatRException(Exception exception)
    {
        // Check if exception has data indicating it came from MediatR pipeline
        // This is a simple heuristic - can be enhanced with custom exception types
        return exception.Data.Contains("MediatRPipeline") ||
               exception.StackTrace?.Contains("MediatR") == true;
    }
}
