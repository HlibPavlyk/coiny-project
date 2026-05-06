namespace Coiny.Application.Common.Results;

/// <summary>
/// Represents the outcome of an operation that has no return value.
/// Sealed — use <see cref="Result{TValue}"/> when a value must be returned on success.
/// </summary>
public sealed class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    private Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);

    public static implicit operator Result(Error error) => Failure(error);
}

/// <summary>
/// Represents the outcome of an operation that returns a value on success.
/// Accessing <see cref="Value"/> on a failed result throws <see cref="InvalidOperationException"/>.
/// Sealed — cannot be subclassed.
/// </summary>
public sealed class Result<TValue>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    internal Result(TValue? value, bool isSuccess, Error error)
    {
        Value = value;
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>The success value. Throws <see cref="InvalidOperationException"/> when <see cref="IsSuccess"/> is false.</summary>
    public TValue Value => IsSuccess
        ? field!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");

    public static implicit operator Result<TValue>(TValue value) => Result.Success(value);

    public static implicit operator Result<TValue>(Error error) => Result.Failure<TValue>(error);
}
