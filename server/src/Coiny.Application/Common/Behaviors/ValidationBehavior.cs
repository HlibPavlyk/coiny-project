using System.Collections.Concurrent;
using System.Reflection;
using Coiny.Application.Common.Results;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Coiny.Application.Common.Behaviors;

/// <summary>
/// Runs all registered <see cref="IValidator{T}"/> instances before the handler.
/// On the first aggregate failure, short-circuits by returning <see cref="Result.Failure"/>
/// or <see cref="Result{TValue}"/> — the handler is never invoked.
/// Validators are run in parallel; multiple failures are aggregated into one description.
/// </summary>
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly ConcurrentDictionary<Type, Func<Error, object>> FailureFactories = new();

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        ValidationContext<TRequest> context = new(request);

        ValidationResult[] results = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        List<ValidationFailure> failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        string code = $"Validation.{failures[0].PropertyName}";
        string description = string.Join("; ", failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}"));
        Error error = Error.Validation(code, description);

        Func<Error, object> factory = FailureFactories.GetOrAdd(typeof(TResponse), BuildFactory);
        return (TResponse)factory(error);
    }

    private static Func<Error, object> BuildFactory(Type responseType)
    {
        if (responseType == typeof(Result))
            return error => Result.Failure(error);

        Type valueType = responseType.GetGenericArguments()[0];
        MethodInfo method = typeof(Result)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == nameof(Result.Failure) && m.IsGenericMethodDefinition)
            .MakeGenericMethod(valueType);

        return error => method.Invoke(null, [error])!;
    }
}
