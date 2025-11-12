using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using MyTodos.BuildingBlocks.Presentation.Constants;

namespace MyTodos.BuildingBlocks.Presentation.ProblemDetails;

/// <summary>
/// Factory for creating RFC 7807 Problem Details responses from exceptions.
/// Maps exceptions to appropriate HTTP status codes and provides consistent error responses.
/// </summary>
public sealed class ProblemDetailsFactory
{
    /// <summary>
    /// Creates a Problem Details response from an exception.
    /// </summary>
    /// <param name="exception">The exception to convert.</param>
    /// <returns>A Problem Details object with appropriate status code and error details.</returns>
    public Microsoft.AspNetCore.Mvc.ProblemDetails CreateFromException(Exception exception)
    {
        return exception switch
        {
            ValidationException validationException => CreateValidationProblemDetails(validationException),
            UnauthorizedAccessException _ => CreateProblemDetails(
                (int)HttpStatusCode.Unauthorized,
                ProblemDetailsConstants.Titles.Unauthorized,
                "Authentication is required to access this resource.",
                ProblemDetailsConstants.Types.Unauthorized),
            KeyNotFoundException _ => CreateProblemDetails(
                (int)HttpStatusCode.NotFound,
                ProblemDetailsConstants.Titles.NotFound,
                "The requested resource was not found.",
                ProblemDetailsConstants.Types.NotFound),
            InvalidOperationException invalidOpException => CreateProblemDetails(
                (int)HttpStatusCode.BadRequest,
                ProblemDetailsConstants.Titles.BadRequest,
                invalidOpException.Message,
                ProblemDetailsConstants.Types.BadRequest),
            ArgumentException argumentException => CreateProblemDetails(
                (int)HttpStatusCode.BadRequest,
                ProblemDetailsConstants.Titles.BadRequest,
                argumentException.Message,
                ProblemDetailsConstants.Types.BadRequest),
            _ => CreateProblemDetails(
                (int)HttpStatusCode.InternalServerError,
                ProblemDetailsConstants.Titles.InternalServerError,
                "An unexpected error occurred. Please contact support if the problem persists.",
                ProblemDetailsConstants.Types.InternalServerError)
        };
    }

    private static Microsoft.AspNetCore.Mvc.ProblemDetails CreateProblemDetails(
        int status,
        string title,
        string detail,
        string type)
    {
        return new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Type = type
        };
    }

    private static Microsoft.AspNetCore.Mvc.ProblemDetails CreateValidationProblemDetails(
        ValidationException validationException)
    {
        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = (int)HttpStatusCode.BadRequest,
            Title = "Validation Error",
            Detail = "One or more validation errors occurred.",
            Type = ProblemDetailsConstants.Types.BadRequest
        };

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
