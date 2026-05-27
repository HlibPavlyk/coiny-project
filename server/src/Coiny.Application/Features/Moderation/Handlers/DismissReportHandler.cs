using Coiny.Application.Abstractions.Data;
using Coiny.Application.Abstractions.Http;
using Coiny.Application.Abstractions.Providers;
using Coiny.Application.Common.Authorization;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Moderation.Requests;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Moderation.Handlers;

/// <summary>
/// Admin dismisses an open report. Idempotency guard: only an <c>Open</c> report can be resolved, so a
/// second dismiss (or dismissing an already-actioned report) returns 409.
/// </summary>
public class DismissReportHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IDateTimeProvider clock)
    : IRequestHandler<DismissReportRequest, Result>
{
    public async Task<Result> Handle(DismissReportRequest request, CancellationToken ct)
    {
        if (!currentUser.CanModerate())
            return Result.Failure(Error.Forbidden("Admin.Forbidden", "Moderator or Administrator role required."));

        Report? report = await db.Reports.FirstOrDefaultAsync(r => r.Id == request.ReportId, ct);
        if (report is null)
            return Result.Failure(Error.NotFound("Report.NotFound", "Report not found."));

        if (report.Status != ReportStatus.Open)
            return Result.Failure(Error.Conflict("Report.AlreadyResolved", "This report is already resolved."));

        report.Status = ReportStatus.Dismissed;
        report.ResolvedAt = clock.UtcNow;
        report.ResolvedByUserId = currentUser.UserId;
        report.ResolutionNote = request.ResolutionNote;

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
