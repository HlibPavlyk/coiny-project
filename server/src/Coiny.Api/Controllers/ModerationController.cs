using Coiny.Application.Common.Authorization;
using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Moderation.BanUser;
using Coiny.Application.Features.Moderation.DismissReport;
using Coiny.Application.Features.Moderation.GetReports;
using Coiny.Application.Features.Moderation.TakeActionOnReport;
using Coiny.Application.Features.Moderation.TakedownLot;
using Coiny.Application.Features.Moderation.UnbanUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coiny.Api.Controllers;

[ApiController]
[Route("api/v1/moderation")]
[Tags("Moderation")]
[Authorize(Roles = RoleNames.ModerationRoles)]
public class ModerationController(IMediator mediator) : ControllerBase
{
    /// <summary>Paginated reports list, filtered by status (null = all). Sort: createdAt/resolvedAt.</summary>
    [HttpPost("reports/search")]
    public Task<Result<Paginated<ReportItemModel>>> SearchReports(
        [FromBody] GetReportsRequest request, CancellationToken ct) =>
        mediator.Send(request, ct);

    /// <summary>Dismiss an open report with an optional resolution note.</summary>
    [HttpPost("reports/{id:guid}/dismiss")]
    public Task<Result> DismissReport(Guid id, [FromBody] DismissReportRequest request, CancellationToken ct) =>
        mediator.Send(request with { ReportId = id }, ct);

    /// <summary>Record that an open report was actioned. The actual takedown/ban is a separate call.</summary>
    [HttpPost("reports/{id:guid}/take-action")]
    public Task<Result> TakeActionOnReport(Guid id, [FromBody] TakeActionOnReportRequest request, CancellationToken ct) =>
        mediator.Send(request with { ReportId = id }, ct);

    /// <summary>Take a lot down (soft-delete) — hides it from listings and drops it from search.</summary>
    [HttpPost("lots/{id:guid}/takedown")]
    public Task<Result> TakedownLot(Guid id, CancellationToken ct) =>
        mediator.Send(new TakedownLotRequest(id), ct);

    /// <summary>Ban a user and run the auto-cleanup pipeline (cancel active lots + in-flight payments).</summary>
    [HttpPost("users/{id:guid}/ban")]
    public Task<Result> BanUser(Guid id, [FromBody] BanUserRequest request, CancellationToken ct) =>
        mediator.Send(request with { UserId = id }, ct);

    /// <summary>Lift a ban. Does not restore cancelled lots or payments.</summary>
    [HttpPost("users/{id:guid}/unban")]
    public Task<Result> UnbanUser(Guid id, CancellationToken ct) =>
        mediator.Send(new UnbanUserRequest(id), ct);
}
