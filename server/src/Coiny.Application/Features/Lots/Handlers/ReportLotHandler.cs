using Coiny.Application.Abstractions.Data;
using Coiny.Application.Abstractions.Providers;
using Coiny.Application.Abstractions.Http;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Lots.Requests;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Coiny.Application.Features.Lots.Handlers;

public class ReportLotHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IIpAddressResolver ipResolver,
    IDateTimeProvider clock,
    IMemoryCache cache)
    : IRequestHandler<ReportLotRequest, Result>
{
    private const int AnonymousLimitPerHour = 3;
    private const int AuthenticatedLimitPerHour = 5;
    private static readonly TimeSpan RateWindow = TimeSpan.FromHours(1);

    public async Task<Result> Handle(ReportLotRequest request, CancellationToken ct)
    {
        bool lotExists = await db.Lots.AnyAsync(l => l.Id == request.LotId, ct);
        if (!lotExists)
            return Result.Failure(Error.NotFound("Lot.NotFound", "Lot not found."));

        Guid? userId = currentUser.IsAuthenticated ? currentUser.UserId : null;
        string? ip = userId is null ? ipResolver.Resolve() : null;

        string cacheKey;
        int limit;
        if (userId is { } uid)
        {
            cacheKey = $"report-rate:user:{uid}";
            limit = AuthenticatedLimitPerHour;
        }
        else
        {
            cacheKey = $"report-rate:ip:{ip ?? "unknown"}";
            limit = AnonymousLimitPerHour;
        }

        int currentCount = cache.TryGetValue(cacheKey, out int existing) ? existing : 0;
        if (currentCount >= limit)
            return Result.Failure(Error.RateLimited(
                "Report.Throttled",
                $"Too many reports — limit is {limit} per hour."));

        cache.Set(cacheKey, currentCount + 1, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = RateWindow,
        });

        db.Reports.Add(new Report
        {
            Id = Guid.NewGuid(),
            LotId = request.LotId,
            ReporterUserId = userId,
            ReporterIp = ip,
            Reason = request.Reason,
            Note = request.Note,
            Status = ReportStatus.Open,
            CreatedAt = clock.UtcNow,
        });

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
