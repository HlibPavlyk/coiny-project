using Coiny.Application.Abstractions.Infrastructure.Data;
using Coiny.Application.Common.Results;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Users.GetPublicProfile;

/// <summary>
/// Public seller profile lookup. Returns 404 for banned users (we deliberately hide ban status —
/// surfacing "this user is banned" leaks moderation signal). LastActiveAt currently mirrors
/// <c>User.UpdatedAt</c> which advances on any auth or profile mutation; if a finer signal is
/// ever needed (e.g., last bid placed), revisit this projection.
/// </summary>
public class GetPublicProfileHandler(IApplicationDbContext db)
    : IRequestHandler<GetPublicProfileRequest, Result<PublicProfileModel>>
{
    public async Task<Result<PublicProfileModel>> Handle(GetPublicProfileRequest request, CancellationToken ct)
    {
        User? user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, ct);

        if (user is null || user.IsBanned)
            return Result.Failure<PublicProfileModel>(
                Error.NotFound("User.NotFound", "User not found."));

        // Aggregate seller stats in one round-trip. Lots authored by this user, grouped by status:
        // - Sold count + average winning bid (CurrentPriceUahKopiykas is the final hammer for Sold)
        // - Active count
        var stats = await db.Lots
            .AsNoTracking()
            .Where(l => l.SellerId == user.Id)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                LotsSold = g.Count(l => l.Status == LotStatus.Sold),
                ActiveLots = g.Count(l => l.Status == LotStatus.Active),
                AvgSalePrice = g.Where(l => l.Status == LotStatus.Sold)
                    .Average(l => (double?)l.CurrentPriceUahKopiykas) ?? 0d,
            })
            .FirstOrDefaultAsync(ct);

        long avgKopiykas = stats is null ? 0L : (long)Math.Round(stats.AvgSalePrice);

        return Result.Success(new PublicProfileModel(
            Id: user.Id,
            DisplayName: user.DisplayName,
            TrustScore: user.TrustScore,
            MemberSince: user.CreatedAt,
            LastActiveAt: user.UpdatedAt,
            LotsSold: stats?.LotsSold ?? 0,
            ActiveLots: stats?.ActiveLots ?? 0,
            AvgSalePriceUahKopiykas: avgKopiykas));
    }
}
