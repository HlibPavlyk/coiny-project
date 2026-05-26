using System.Text.Json;
using Coiny.Application.Common.Json;

namespace Coiny.Application.Features.Auth.Events;

/// <summary>
/// Payload schema for <c>EmailOutboxEvent</c> rows of type <c>EmailVerificationRequested</c>.
/// Producer: register / resend handlers. Consumer: <c>EmailOutboxFlushJob</c>.
/// </summary>
public sealed record EmailVerificationPayload(Guid UserId, string ToAddress, string Token, string TokenHash)
{
    public const string EventType = "EmailVerificationRequested";

    public string Serialize() => JsonSerializer.Serialize(this, JsonDefaults.Options);

    public static EmailVerificationPayload Deserialize(string json) =>
        JsonSerializer.Deserialize<EmailVerificationPayload>(json, JsonDefaults.Options)
            ?? throw new InvalidOperationException("EmailVerificationPayload deserialization returned null.");
}
