using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MyTodos.BuildingBlocks.Presentation.ProblemDetails;

namespace MyTodos.BuildingBlocks.Presentation.Middleware;

/// <summary>
/// Global exception handler middleware that catches unhandled exceptions and converts them to Problem Details responses.
/// Logs exceptions that occur outside the MediatR pipeline (pipeline exceptions are already logged by LoggingBehavior).
/// </summary>
public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    ProblemDetailsFactory problemDetailsFactory) : IMiddleware
{
    /// <summary>
    /// Invokes the middleware to handle exceptions in the HTTP pipeline.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="next">The next middleware in the pipeline.</param>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            // Execute the rest of the pipeline
            await next(context);
        }
        catch (Exception ex)
        {
            // Log exception only if it occurred outside MediatR pipeline
            // MediatR exceptions are already logged by LoggingBehavior
            if (!IsMediatRException(ex))
            {
                logger.LogError(
                    ex,
                    "Unhandled exception occurred outside MediatR pipeline at {Path}",
                    context.Request.Path);
            }
            else
            {
                // MediatR exception - already logged, just converting to response
                logger.LogDebug(
                    "Converting MediatR exception to Problem Details response for {Path}",
                    context.Request.Path);
            }

            // Convert exception to Problem Details and send response
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Create Problem Details response from exception
        var problemDetails = problemDetailsFactory.CreateFromException(exception);

        // Set response status code and content type
        context.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/problem+json";

        // Serialize and write Problem Details to response
        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        await context.Response.WriteAsync(json);
    }

    private static bool IsMediatRException(Exception exception)
    {
        // Check if exception has data indicating it came from MediatR pipeline
        // This is a simple heuristic - can be enhanced with custom exception types
        return exception.Data.Contains("MediatRPipeline") ||
               exception.StackTrace?.Contains("MediatR") == true;
    }
}
