using System.Collections.Concurrent;
using System.Reflection;
using Coiny.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Coiny.Api.Filters;

public class ResultTransformFilter : IAsyncResultFilter
{
    private static readonly ConcurrentDictionary<Type, ResultReflector> Reflectors = new();

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult { Value: not null } objectResult)
        {
            IActionResult? transformed = TryTransform(objectResult.Value, context.HttpContext.Request.Path);
            if (transformed is not null)
                context.Result = transformed;
        }

        await next();
    }

    private static IActionResult? TryTransform(object value, PathString path)
    {
        if (value is Result nonGeneric)
            return BuildResponse(nonGeneric.IsSuccess, nonGeneric.Error, successValue: null, path);

        Type type = value.GetType();
        if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Result<>))
            return null;

        ResultReflector reflector = Reflectors.GetOrAdd(type, t => new ResultReflector(t));
        bool isSuccess = reflector.GetIsSuccess(value);
        return BuildResponse(isSuccess, reflector.GetError(value), isSuccess ? reflector.GetValue(value) : null, path);
    }

    private static IActionResult BuildResponse(bool isSuccess, Error error, object? successValue, PathString path)
    {
        if (isSuccess)
            return successValue is null ? new OkResult() : new OkObjectResult(successValue);

        ProblemDetails problem = new()
        {
            Title = GetTitle(error.Type),
            Detail = error.Description,
            Status = GetStatus(error.Type),
            Instance = path,
        };
        return new ObjectResult(problem) { StatusCode = problem.Status };
    }

    internal static int GetStatus(ErrorType type) => type switch
    {
        ErrorType.Validation   => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden    => StatusCodes.Status403Forbidden,
        ErrorType.NotFound     => StatusCodes.Status404NotFound,
        ErrorType.Conflict     => StatusCodes.Status409Conflict,
        ErrorType.RateLimited  => StatusCodes.Status429TooManyRequests,
        ErrorType.ExternalService => StatusCodes.Status502BadGateway,
        _                      => StatusCodes.Status500InternalServerError,
    };

    private static string GetTitle(ErrorType type) => type switch
    {
        ErrorType.Validation   => "Validation Error",
        ErrorType.Unauthorized => "Unauthorized",
        ErrorType.Forbidden    => "Forbidden",
        ErrorType.NotFound     => "Resource Not Found",
        ErrorType.Conflict     => "Conflict",
        ErrorType.RateLimited  => "Too Many Requests",
        ErrorType.ExternalService => "Bad Gateway",
        _                      => "Internal Server Error",
    };

    private sealed class ResultReflector
    {
        private readonly Func<object, bool> _isSuccess;
        private readonly Func<object, Error> _error;
        private readonly Func<object, object?> _value;

        public ResultReflector(Type type)
        {
            PropertyInfo isSuccessProp = type.GetProperty(nameof(Result<int>.IsSuccess))!;
            PropertyInfo errorProp = type.GetProperty(nameof(Result<int>.Error))!;
            PropertyInfo valueProp = type.GetProperty(nameof(Result<int>.Value))!;

            _isSuccess = obj => (bool)isSuccessProp.GetValue(obj)!;
            _error = obj => (Error)errorProp.GetValue(obj)!;
            _value = obj => valueProp.GetValue(obj);
        }

        public bool GetIsSuccess(object obj) => _isSuccess(obj);
        public Error GetError(object obj) => _error(obj);
        public object? GetValue(object obj) => _value(obj);
    }
}
