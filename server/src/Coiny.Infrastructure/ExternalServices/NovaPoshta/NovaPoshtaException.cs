namespace Coiny.Infrastructure.ExternalServices.NovaPoshta;

/// <summary>
/// Thrown when Nova Poshta responds with <c>success: false</c>. The <see cref="Errors"/>
/// list contains the human-readable messages from the API response.
/// </summary>
public class NovaPoshtaException(IReadOnlyList<string> errors)
    : Exception($"Nova Poshta API error: {string.Join("; ", errors)}")
{
    public IReadOnlyList<string> Errors { get; } = errors;
}
