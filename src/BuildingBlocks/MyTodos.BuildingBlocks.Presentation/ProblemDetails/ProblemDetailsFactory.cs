using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using MyTodos.BuildingBlocks.Presentation.Constants;
using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.BuildingBlocks.Presentation.ProblemDetails;

/// <summary>
/// Factory for creating RFC 7807 Problem Details responses from exceptions.
/// Maps exceptions to appropriate HTTP status codes and provides consistent error responses.
/// Environment-aware error exposure:
/// - Development: Shows full exception details (ToString()) for all exceptions
/// - Production: Shows exception.Message only for DomainException and ValidationException (safe to expose)
/// - Production: Shows generic messages for infrastructure exceptions (SqlException, etc.)
/// </summary>
public sealed class ProblemDetailsFactory
{
    private readonly IHostEnvironment _environment;

    public ProblemDetailsFactory(IHostEnvironment environment)
    {
        _environment = environment;
    }

    /// <summary>
    /// Creates a Problem Details response from an exception.
    /// </summary>
    /// <param name="httpContext">The HTTP context for the request.</param>
    /// <param name="exception">The exception to convert.</param>
    /// <returns>A Problem Details object with appropriate status code and error details.</returns>
    public Microsoft.AspNetCore.Mvc.ProblemDetails CreateFromException(HttpContext httpContext, Exception exception)
    {
        return exception switch
        {
            DomainException _ => CreateProblemDetails(
                httpContext,
                exception,
                (int)HttpStatusCode.BadRequest,
                ProblemDetailsConstants.Titles.BadRequest,
                "A domain rule violation occurred.",
                ProblemDetailsConstants.Types.BadRequest),
            ValidationException validationException => CreateValidationProblemDetails(httpContext, validationException),
            UnauthorizedAccessException _ => CreateProblemDetails(
                httpContext,
                exception,
                (int)HttpStatusCode.Unauthorized,
                ProblemDetailsConstants.Titles.Unauthorized,
                "Authentication is required to access this resource.",
                ProblemDetailsConstants.Types.Unauthorized),
            KeyNotFoundException _ => CreateProblemDetails(
                httpContext,
                exception,
                (int)HttpStatusCode.NotFound,
                ProblemDetailsConstants.Titles.NotFound,
                "The requested resource was not found.",
                ProblemDetailsConstants.Types.NotFound),
            InvalidOperationException _ => CreateProblemDetails(
                httpContext,
                exception,
                (int)HttpStatusCode.BadRequest,
                ProblemDetailsConstants.Titles.BadRequest,
                "A business rule violation occurred.",
                ProblemDetailsConstants.Types.BadRequest),
            ArgumentException _ => CreateProblemDetails(
                httpContext,
                exception,
                (int)HttpStatusCode.BadRequest,
                ProblemDetailsConstants.Titles.BadRequest,
                "Invalid argument provided.",
                ProblemDetailsConstants.Types.BadRequest),
            _ => CreateProblemDetails(
                httpContext,
                exception,
                (int)HttpStatusCode.InternalServerError,
                ProblemDetailsConstants.Titles.InternalServerError,
                "An unexpected error occurred. Please contact support if the problem persists.",
                ProblemDetailsConstants.Types.InternalServerError)
        };
    }

    private Microsoft.AspNetCore.Mvc.ProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        Exception exception,
        int status,
        string title,
        string genericDetail,
        string type)
    {
        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = GetDetailMessage(exception, genericDetail),
            Type = type
        };

        // Add traceId for request correlation
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        // In development, include exception data for debugging
        if (_environment.IsDevelopment() && exception.Data.Count > 0)
        {
            problemDetails.Extensions["data"] = exception.Data;
        }

        return problemDetails;
    }

    /// <summary>
    /// Determines what error detail message to show based on exception type and environment.
    /// </summary>
    /// <param name="exception">The exception to get details from.</param>
    /// <param name="genericDetail">Generic fallback message for infrastructure exceptions.</param>
    /// <returns>The detail message to include in Problem Details response.</returns>
    private string GetDetailMessage(Exception exception, string genericDetail)
    {
        // In development, always show full exception details for debugging
        if (_environment.IsDevelopment())
        {
            return exception.ToString();
        }

        // In production, only show exception message for domain exceptions (safe to expose)
        // DomainException messages are controlled by developers and safe for end users
        if (exception is DomainException)
        {
            return exception.Message;
        }

        // For infrastructure/system exceptions, use generic message to avoid leaking internals
        // Examples: SqlException, IOException, NullReferenceException
        return genericDetail;
    }

    private Microsoft.AspNetCore.Mvc.ProblemDetails CreateValidationProblemDetails(
        HttpContext httpContext,
        ValidationException validationException)
    {
        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = (int)HttpStatusCode.BadRequest,
            Title = "Validation Error",
            Detail = "One or more validation errors occurred.",
            Type = ProblemDetailsConstants.Types.BadRequest
        };

        // Add traceId for request correlation
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        // Add validation errors as extensions
        var errors = validationException.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        problemDetails.Extensions["errors"] = errors;

        return problemDetails;
    }
}
