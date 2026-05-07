using System.Security.Cryptography;
using System.Text;

namespace Coiny.Application.Features.Auth;

/// <summary>Generates and hashes single-use email verification tokens.</summary>
internal static class VerificationTokenFactory
{
    private const int TokenByteLength = 32;

    /// <summary>Returns a base64url-encoded random token suitable for an email link.</summary>
    public static string NewRawToken()
    {
        Span<byte> bytes = stackalloc byte[TokenByteLength];
        RandomNumberGenerator.Fill(bytes);
        return Base64UrlEncode(bytes);
    }

    /// <summary>SHA-256 hex of the raw token. Length 64.</summary>
    public static string Hash(string rawToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

    private static string Base64UrlEncode(ReadOnlySpan<byte> bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
