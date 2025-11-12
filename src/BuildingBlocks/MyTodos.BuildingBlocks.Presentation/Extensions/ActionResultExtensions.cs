using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyTodos.BuildingBlocks.Presentation.Constants;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.BuildingBlocks.Presentation.Extensions;

public static class ActionResultExtensions
{
    public static ActionResult ToProblemDetails(this Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Cannot convert a successful result to ProblemDetails. Only failed results can be converted.");
        }

        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails()
        {
            Status = MapStatusCode(result.ErrorType),
            Title = MapTitle(result.ErrorType),
            Type = MapType(result.ErrorType),
            Extensions = new Dictionary<string, object?>
            {
                { "errors", result.ErrorDescriptions }
            }
        };

        return new BadRequestObjectResult(problemDetails)
        {
            StatusCode = problemDetails.Status
        };
    }

    public static ActionResult ToProblemDetails<TValue>(this Result<TValue> result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Cannot convert a successful result to ProblemDetails. Only failed results can be converted.");
        }

        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails()
        {
            Status = MapStatusCode(result.ErrorType),
            Title = MapTitle(result.ErrorType),
            Type = MapType(result.ErrorType),
            Extensions = new Dictionary<string, object?>
            {
                { "errors", result.ErrorDescriptions }
            }
        };

        return new BadRequestObjectResult(problemDetails)
        {
            StatusCode = problemDetails.Status
        };
    }

    public static ActionResult ToActionResult<TValue>(this Result<TValue> result)
    {
        return result.IsSuccess
            ? new OkObjectResult(result.Value)
            : result.ToProblemDetails();
    }

    public static ActionResult ToActionResult(this Result result)
    {
        return result.IsSuccess
            ? new StatusCodeResult(StatusCodes.Status204NoContent)
            : result.ToProblemDetails();
    }

    public static ActionResult ToCreatedAtRouteResult<TValue>(this Result<TValue> result,
        string route)
    {
        return result.IsSuccess
            ? new CreatedAtRouteResult(route, result.Value)
            : result.ToProblemDetails();
    }

    private static int MapStatusCode(ErrorType errorType)
    {
        return errorType switch
        {
            ErrorType.BadRequest => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NullValue => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static string MapTitle(ErrorType errorType)
    {
        return errorType switch
        {
            ErrorType.BadRequest => ProblemDetailsConstants.Titles.BadRequest,
            ErrorType.NotFound => ProblemDetailsConstants.Titles.NotFound,
            ErrorType.Conflict => ProblemDetailsConstants.Titles.Conflict,
            ErrorType.Unauthorized => ProblemDetailsConstants.Titles.Unauthorized,
            ErrorType.Forbidden => ProblemDetailsConstants.Titles.Forbidden,
            ErrorType.NullValue => ProblemDetailsConstants.Titles.UnprocessableEntity,
            _ => ProblemDetailsConstants.Titles.InternalServerError
        };
    }

    private static string MapType(ErrorType errorType)
    {
        return errorType switch
        {
            ErrorType.BadRequest => ProblemDetailsConstants.Types.BadRequest,
            ErrorType.NotFound => ProblemDetailsConstants.Types.NotFound,
            ErrorType.Conflict => ProblemDetailsConstants.Types.Conflict,
            ErrorType.Unauthorized => ProblemDetailsConstants.Types.Unauthorized,
            ErrorType.Forbidden => ProblemDetailsConstants.Types.Forbidden,
            ErrorType.NullValue => ProblemDetailsConstants.Types.UnprocessableEntity,
            _ => ProblemDetailsConstants.Types.InternalServerError
        };
    }
}