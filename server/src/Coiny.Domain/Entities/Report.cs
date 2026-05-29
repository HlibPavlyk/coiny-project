using Coiny.Domain.Enums;

namespace Coiny.Domain.Entities;

public class Report
{
    public Guid Id { get; set; }

    public Guid LotId { get; set; }

    /// <summary>FK → User.Id; null for anonymous reports.</summary>
    public Guid? ReporterUserId { get; set; }

    /// <summary>Captured for anonymous reports; client IP via X-Forwarded-For when behind Coolify proxy.</summary>
    public string? ReporterIp { get; set; }

    public ReportReason Reason { get; set; }

    /// <summary>≤ 500 chars.</summary>
    public string? Note { get; set; }

    public ReportStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    /// <summary>FK → User.Id (the admin who resolved).</summary>
    public Guid? ResolvedByUserId { get; set; }

    /// <summary>≤ 500 chars.</summary>
    public string? ResolutionNote { get; set; }

    public virtual Lot? Lot { get; set; }

    public virtual User? Reporter { get; set; }

    public virtual User? Resolver { get; set; }
}
