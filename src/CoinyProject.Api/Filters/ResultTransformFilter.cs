using CoinyProject.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CoinyProject.Api.Filters;

public class ResultTransformFilter : IAsyncResultFilter
{
    private readonly ILogger<ResultTransformFilter> _logger;

    public ResultTransformFilter(ILogger<ResultTransformFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult objectResult && objectResult.Value is Result result)
        {
            context.Result = TransformResult(result, objectResult.Value, context.HttpContext.Request.Path);
        }

        await next();
    }

    private IActionResult TransformResult(Result result, object originalValue, PathString requestPath)
    {
        if (result.IsSuccess)
        {
            var valueProperty = originalValue.GetType().GetProperty("Value");
            if (valueProperty != null)
            {
                var actualValue = valueProperty.GetValue(originalValue);
                return new OkObjectResult(actualValue);
            }
            return new OkResult();
        }

        _logger.LogWarning(
            "Request {Path} failed with {ErrorType}: {ErrorMessage}",
            requestPath,
            result.Error.Type,
            result.Error.GetDetailedMessage());

        var problemDetails = new ProblemDetails
        {
            Title = GetTitle(result.Error.Type),
            Detail = result.Error.Message,
            Status = GetStatusCode(result.Error.Type),
            Instance = requestPath
        };

        return new ObjectResult(problemDetails)
        {
            StatusCode = problemDetails.Status
        };
    }

    private static int GetStatusCode(ErrorType errorType) => errorType switch
    {
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        _ => StatusCodes.Status500InternalServerError
    };

    private static string GetTitle(ErrorType errorType) => errorType switch
    {
        ErrorType.NotFound => "Resource Not Found",
        ErrorType.Validation => "Validation Error",
        ErrorType.Conflict => "Conflict",
        ErrorType.Unauthorized => "Unauthorized",
        ErrorType.Forbidden => "Forbidden",
        _ => "Internal Server Error"
    };
}
