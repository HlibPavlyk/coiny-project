using System.Runtime.CompilerServices;

namespace Coiny.Application.Common.Results;

/// <summary>
/// Immutable, value-equal error descriptor produced by application handlers.
/// Use the static factories (<see cref="NotFound"/>, <see cref="Validation"/>, etc.) — never construct directly.
/// </summary>
public sealed record Error
{
    /// <summary>Machine-readable identifier, e.g. "Lot.NotFound" or "Bid.BelowMinimum".</summary>
    public string Code { get; init; }

    /// <summary>Human-readable description surfaced in the API ProblemDetails body.</summary>
    public string Description { get; init; }

    public ErrorType Type { get; init; }

    /// <summary>Source file path captured via CallerFilePath; null when not available.</summary>
    public string? Source { get; init; }

    public int? Line { get; init; }

    /// <summary>Calling member name captured via CallerMemberName; null when not available.</summary>
    public string? MemberName { get; init; }

    private Error(
        string code,
        string description,
        ErrorType type,
        string? source = null,
        int? line = null,
        string? memberName = null)
    {
        Code = code;
        Description = description;
        Type = type;
        Source = source;
        Line = line;
        MemberName = memberName;
    }

    /// <summary>Sentinel used internally by successful Results. Never inspect this on a success path.</summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);

    public static Error Validation(
        string code,
        string description,
        [CallerFilePath] string? source = null,
        [CallerLineNumber] int? line = null,
        [CallerMemberName] string? memberName = null) =>
        new(code, description, ErrorType.Validation, source, line, memberName);

    public static Error NotFound(
        string code,
        string description,
        [CallerFilePath] string? source = null,
        [CallerLineNumber] int? line = null,
        [CallerMemberName] string? memberName = null) =>
        new(code, description, ErrorType.NotFound, source, line, memberName);

    public static Error Conflict(
        string code,
        string description,
        [CallerFilePath] string? source = null,
        [CallerLineNumber] int? line = null,
        [CallerMemberName] string? memberName = null) =>
        new(code, description, ErrorType.Conflict, source, line, memberName);

    public static Error Unauthorized(
        string code,
        string description,
        [CallerFilePath] string? source = null,
        [CallerLineNumber] int? line = null,
        [CallerMemberName] string? memberName = null) =>
        new(code, description, ErrorType.Unauthorized, source, line, memberName);

    public static Error Forbidden(
        string code,
        string description,
        [CallerFilePath] string? source = null,
        [CallerLineNumber] int? line = null,
        [CallerMemberName] string? memberName = null) =>
        new(code, description, ErrorType.Forbidden, source, line, memberName);

    /// <summary>
    /// Use for unexpected failures that map to HTTP 500.
    /// Named <c>Internal</c> to avoid the double-word ambiguity of <c>Result.Failure(Error.Failure(...))</c>.
    /// </summary>
    public static Error Internal(
        string code,
        string description,
        [CallerFilePath] string? source = null,
        [CallerLineNumber] int? line = null,
        [CallerMemberName] string? memberName = null) =>
        new(code, description, ErrorType.Internal, source, line, memberName);

    public static Error RateLimited(
        string code,
        string description,
        [CallerFilePath] string? source = null,
        [CallerLineNumber] int? line = null,
        [CallerMemberName] string? memberName = null) =>
        new(code, description, ErrorType.RateLimited, source, line, memberName);

    public string GetDetailedMessage()
    {
        if (Source is null)
            return $"[{Code}] {Description}";

        var fileName = Path.GetFileName(Source);
        return $"[{Code}] {Description} (at {fileName}:{Line} in {MemberName})";
    }

    public override string ToString() => GetDetailedMessage();
}
