using Coiny.Application.Common.Results;

namespace Coiny.Application.Common.Extensions;

/// <summary>Chainable extension methods for the Result pattern.</summary>
public static class ResultExtensions
{
    /// <summary>Transforms the success value; propagates failure unchanged.</summary>
    public static Result<TOut> Map<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> mapper) =>
        result.IsSuccess
            ? Result.Success(mapper(result.Value))
            : Result.Failure<TOut>(result.Error);

    /// <summary>Chains to another result-returning operation (async); propagates failure unchanged.</summary>
    public static async Task<Result<TOut>> Bind<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<Result<TOut>>> binder) =>
        result.IsSuccess
            ? await binder(result.Value)
            : Result.Failure<TOut>(result.Error);

    /// <summary>Chains to another result-returning operation (sync); propagates failure unchanged.</summary>
    public static Result<TOut> Bind<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Result<TOut>> binder) =>
        result.IsSuccess
            ? binder(result.Value)
            : Result.Failure<TOut>(result.Error);

    /// <summary>Projects the result into a single value using one of two selector functions.</summary>
    public static T Match<TIn, T>(
        this Result<TIn> result,
        Func<TIn, T> onSuccess,
        Func<Error, T> onFailure) =>
        result.IsSuccess
            ? onSuccess(result.Value)
            : onFailure(result.Error);

    /// <summary>Executes a side-effect on success; returns the original result for continued chaining.</summary>
    public static async Task<Result<T>> Tap<T>(
        this Result<T> result,
        Func<T, Task> action)
    {
        if (result.IsSuccess)
            await action(result.Value);

        return result;
    }

    /// <summary>Converts a nullable reference to a Result, returning <see cref="Error.NotFound"/> when null.</summary>
    public static Result<T> ToResult<T>(this T? value, string code, string description) where T : class =>
        value is not null
            ? Result.Success(value)
            : Result.Failure<T>(Error.NotFound(code, description));

    /// <summary>Validates a predicate on the success value; returns <see cref="Error.Validation"/> on failure.</summary>
    public static Result<T> Ensure<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        Error error)
    {
        if (result.IsFailure)
            return result;

        return predicate(result.Value)
            ? result
            : Result.Failure<T>(error);
    }
}
