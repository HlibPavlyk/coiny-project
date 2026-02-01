using CoinyProject.Application.Common.Results;

namespace CoinyProject.Application.Common.Extensions;

/// <summary>
/// Extension methods for Result pattern
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Maps a successful result to a new result with a different value
    /// </summary>
    public static Result<TOut> Map<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> mapper)
    {
        return result.IsSuccess
            ? Result.Success(mapper(result.Value))
            : Result.Failure<TOut>(result.Error);
    }

    /// <summary>
    /// Binds a result to another result-returning operation
    /// </summary>
    public static async Task<Result<TOut>> Bind<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<Result<TOut>>> binder)
    {
        return result.IsSuccess
            ? await binder(result.Value)
            : Result.Failure<TOut>(result.Error);
    }

    /// <summary>
    /// Binds a result to another result-returning operation (synchronous)
    /// </summary>
    public static Result<TOut> Bind<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Result<TOut>> binder)
    {
        return result.IsSuccess
            ? binder(result.Value)
            : Result.Failure<TOut>(result.Error);
    }

    /// <summary>
    /// Matches result and executes appropriate function
    /// </summary>
    public static T Match<TIn, T>(
        this Result<TIn> result,
        Func<TIn, T> onSuccess,
        Func<Error, T> onFailure)
    {
        return result.IsSuccess
            ? onSuccess(result.Value)
            : onFailure(result.Error);
    }

    /// <summary>
    /// Executes action based on result state
    /// </summary>
    public static async Task<Result<T>> Tap<T>(
        this Result<T> result,
        Func<T, Task> action)
    {
        if (result.IsSuccess)
        {
            await action(result.Value);
        }

        return result;
    }

    /// <summary>
    /// Converts nullable value to Result
    /// </summary>
    public static Result<T> ToResult<T>(this T value, string errorMessage) where T : class
    {
        return value is not null
            ? Result.Success(value)
            : Result.Failure<T>(Error.NotFound(errorMessage));
    }

    /// <summary>
    /// Ensures a condition is met, otherwise returns failure
    /// </summary>
    public static Result<T> Ensure<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        string errorMessage)
    {
        if (result.IsFailure)
            return result;

        return predicate(result.Value)
            ? result
            : Result.Failure<T>(Error.Validation(errorMessage));
    }
}