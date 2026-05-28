namespace Coiny.Application.Features.Users.GetPublicProfile;

/// <summary>
/// Read-only seller profile returned to anonymous and authenticated callers alike.
/// Excludes anything personally identifying beyond displayName (no email, no address).
/// </summary>
public record PublicProfileModel(
    Guid Id,
    string DisplayName,
    int TrustScore,
    DateTime MemberSince,
    DateTime LastActiveAt,
    int LotsSold,
    int ActiveLots,
    long AvgSalePriceUahKopiykas);
