using Coiny.Application.Abstractions.Data;
using Coiny.Application.Abstractions.Http;
using Coiny.Application.Abstractions.Providers;
using Coiny.Application.Common.Authorization;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Admin.Requests;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Admin.Handlers;

/// <summary>
/// Admin records that a report was actioned. This handler only flips the resolution state and stores
/// the note; the actual mutation (delete lot / ban user) is a separate explicit admin call, so the
/// audit trail stays clean. Only an <c>Open</c> report can be actioned (409 otherwise).
/// </summary>
public class TakeActionOnReportHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IDateTimeProvider clock)
    : IRequestHandler<TakeActionOnReportRequest, Result>
{
    public async Task<Result> Handle(TakeActionOnReportRequest request, CancellationToken ct)
    {
        if (!currentUser.Roles.Contains(RoleNames.Admin))
            return Result.Failure(Error.Forbidden("Admin.Forbidden", "Administrator role required."));

        Report? report = await db.Reports.FirstOrDefaultAsync(r => r.Id == request.ReportId, ct);
        if (report is null)
            return Result.Failure(Error.NotFound("Report.NotFound", "Report not found."));

        if (report.Status != ReportStatus.Open)
            return Result.Failure(Error.Conflict("Report.AlreadyResolved", "This report is already resolved."));

        report.Status = ReportStatus.ActionTaken;
        report.ResolvedAt = clock.UtcNow;
        report.ResolvedByUserId = currentUser.UserId;
        report.ResolutionNote = request.ResolutionNote;

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
