using Coiny.Domain.Enums;

namespace Coiny.Application.Features.Moderation.Models;

/// <summary>The lot a report targets, as shown in the admin reports table.</summary>
public record ReportLotInfo(Guid Id, string Title, string CoverImageUrl);

/// <summary>
/// One row of the admin reports surface. Per <c>/docs/02-api-contracts.md</c> §8.
/// <see cref="ReporterDisplayName"/> is the reporter's name for authenticated reports and null for
/// anonymous ones, where <see cref="ReporterIp"/> carries the captured IP instead.
/// </summary>
public record ReportItemModel(
    Guid Id,
    ReportLotInfo Lot,
    string? ReporterDisplayName,
    string? ReporterIp,
    ReportReason Reason,
    string? Note,
    ReportStatus Status,
    DateTime CreatedAt,
    DateTime? ResolvedAt);
