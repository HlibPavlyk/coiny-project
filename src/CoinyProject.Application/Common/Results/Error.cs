using System.Runtime.CompilerServices;

namespace CoinyProject.Application.Common.Results;

/// <summary>
/// Represents an error with message, type and source location
/// </summary>
public sealed record Error
{
    public string Message { get; init; }
    public ErrorType Type { get; init; }
    public string Source { get; init; }
    public int? Line { get; init; }
    public string MemberName { get; init; }

    private Error(
        string message,
        ErrorType type,
        string source = null,
        int? line = null,
        string memberName = null)
    {
        Message = message;
        Type = type;
        Source = source;
        Line = line;
        MemberName = memberName;
    }

    public static readonly Error None = new(string.Empty, ErrorType.None);

    public static Error NotFound(
        string message,
        [CallerFilePath] string source = null,
        [CallerLineNumber] int? line = null,
        [CallerMemberName] string memberName = null) =>
        new(message, ErrorType.NotFound, source, line, memberName);

    public static Error Validation(
        string message,
        [CallerFilePath] string source = null,
        [CallerLineNumber] int? line = null,
        [CallerMemberName] string memberName = null) =>
        new(message, ErrorType.Validation, source, line, memberName);

    public static Error Conflict(
        string message,
        [CallerFilePath] string source = null,
        [CallerLineNumber] int? line = null,
        [CallerMemberName] string memberName = null) =>
        new(message, ErrorType.Conflict, source, line, memberName);

    public static Error Failure(
        string message,
        [CallerFilePath] string source = null,
        [CallerLineNumber] int? line = null,
        [CallerMemberName] string memberName = null) =>
        new(message, ErrorType.Failure, source, line, memberName);

    public static Error Unauthorized(
        string message,
        [CallerFilePath] string source = null,
        [CallerLineNumber] int? line = null,
        [CallerMemberName] string memberName = null) =>
        new(message, ErrorType.Unauthorized, source, line, memberName);

    public static Error Forbidden(
        string message,
        [CallerFilePath] string source = null,
        [CallerLineNumber] int? line = null,
        [CallerMemberName] string memberName = null) =>
        new(message, ErrorType.Forbidden, source, line, memberName);

    /// <summary>
    /// Returns formatted error information for logging
    /// </summary>
    public string GetDetailedMessage()
    {
        if (Source is null)
            return Message;

        var fileName = Path.GetFileName(Source);
        return $"{Message} (at {fileName}:{Line} in {MemberName})";
    }

    public override string ToString() => GetDetailedMessage();
}