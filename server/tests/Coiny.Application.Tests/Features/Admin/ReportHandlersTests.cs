using Coiny.Application.Abstractions.Http;
using Coiny.Application.Abstractions.Providers;
using Coiny.Application.Common.Authorization;
using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Admin.Handlers;
using Coiny.Application.Features.Admin.Models;
using Coiny.Application.Features.Admin.Requests;
using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using Coiny.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Coiny.Application.Tests.Features.Admin;

public class ReportHandlersTests
{
    private static readonly DateTime BaseTime = new(2026, 5, 20, 12, 0, 0, DateTimeKind.Utc);

    private readonly Guid _adminId = Guid.NewGuid();

    // ── GetReportsHandler ──────────────────────────────────────────────────

    [Fact]
    public async Task GetReports_non_admin_is_forbidden()
    {
        using var ctx = NewDb();
        var handler = new GetReportsHandler(ctx, NonAdmin());

        Result<Paginated<ReportItemModel>> result = await handler.Handle(new GetReportsRequest(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public async Task GetReports_filters_by_status_and_defaults_to_newest_first()
    {
        using var ctx = NewDb();
        Guid lot = SeedLot(ctx);
        SeedReport(ctx, lot, ReportStatus.Open, BaseTime.AddMinutes(10));
        SeedReport(ctx, lot, ReportStatus.Open, BaseTime.AddMinutes(30)); // newest
        SeedReport(ctx, lot, ReportStatus.Dismissed, BaseTime.AddMinutes(20));
        await ctx.SaveChangesAsync();

        var handler = new GetReportsHandler(ctx, Admin());
        Result<Paginated<ReportItemModel>> result = await handler.Handle(
            new GetReportsRequest { Filters = new GetReportsFilters { Status = ReportStatus.Open } },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(2);
        result.Value.Items.Should().OnlyContain(r => r.Status == ReportStatus.Open);
        // Default sort is createdAt Desc → the 30-minute report comes first.
        result.Value.Items[0].CreatedAt.Should().Be(BaseTime.AddMinutes(30));
    }

    [Fact]
    public async Task GetReports_null_status_returns_all_states()
    {
        using var ctx = NewDb();
        Guid lot = SeedLot(ctx);
        SeedReport(ctx, lot, ReportStatus.Open, BaseTime.AddMinutes(10));
        SeedReport(ctx, lot, ReportStatus.Dismissed, BaseTime.AddMinutes(20));
        SeedReport(ctx, lot, ReportStatus.ActionTaken, BaseTime.AddMinutes(30));
        await ctx.SaveChangesAsync();

        var handler = new GetReportsHandler(ctx, Admin());
        Result<Paginated<ReportItemModel>> result = await handler.Handle(new GetReportsRequest(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetReports_surfaces_reporter_name_or_ip()
    {
        using var ctx = NewDb();
        Guid lot = SeedLot(ctx);
        Guid reporterId = SeedUser(ctx, "Alice");
        SeedReport(ctx, lot, ReportStatus.Open, BaseTime.AddMinutes(10), reporterUserId: reporterId);
        SeedReport(ctx, lot, ReportStatus.Open, BaseTime.AddMinutes(20), reporterIp: "203.0.113.7");
        await ctx.SaveChangesAsync();

        var handler = new GetReportsHandler(ctx, Admin());
        Result<Paginated<ReportItemModel>> result = await handler.Handle(
            new GetReportsRequest { Count = 50 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        ReportItemModel named = result.Value.Items.Single(r => r.ReporterDisplayName == "Alice");
        named.ReporterIp.Should().BeNull();
        ReportItemModel anon = result.Value.Items.Single(r => r.ReporterIp == "203.0.113.7");
        anon.ReporterDisplayName.Should().BeNull();
    }

    [Fact]
    public async Task GetReports_unknown_sort_column_returns_validation_error()
    {
        using var ctx = NewDb();
        var handler = new GetReportsHandler(ctx, Admin());

        Result<Paginated<ReportItemModel>> result = await handler.Handle(
            new GetReportsRequest { SortBy = [new SortByModel("reason", SortDirection.Asc)] },
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Sort.InvalidColumn");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    // ── DismissReportHandler ───────────────────────────────────────────────

    [Fact]
    public async Task Dismiss_flips_state_and_records_resolution()
    {
        using var ctx = NewDb();
        Guid lot = SeedLot(ctx);
        Guid reportId = SeedReport(ctx, lot, ReportStatus.Open, BaseTime);
        await ctx.SaveChangesAsync();

        var handler = new DismissReportHandler(ctx, Admin(), new TestClock(BaseTime.AddHours(1)));
        Result result = await handler.Handle(new DismissReportRequest(reportId, "Not a real issue."), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        Report stored = await ctx.Reports.SingleAsync(r => r.Id == reportId);
        stored.Status.Should().Be(ReportStatus.Dismissed);
        stored.ResolvedAt.Should().Be(BaseTime.AddHours(1));
        stored.ResolvedByUserId.Should().Be(_adminId);
        stored.ResolutionNote.Should().Be("Not a real issue.");
    }

    [Fact]
    public async Task Dismiss_already_resolved_returns_conflict()
    {
        using var ctx = NewDb();
        Guid lot = SeedLot(ctx);
        Guid reportId = SeedReport(ctx, lot, ReportStatus.Dismissed, BaseTime);
        await ctx.SaveChangesAsync();

        var handler = new DismissReportHandler(ctx, Admin(), new TestClock(BaseTime));
        Result result = await handler.Handle(new DismissReportRequest(reportId, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Report.AlreadyResolved");
        result.Error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task Dismiss_missing_report_returns_not_found()
    {
        using var ctx = NewDb();
        var handler = new DismissReportHandler(ctx, Admin(), new TestClock(BaseTime));

        Result result = await handler.Handle(new DismissReportRequest(Guid.NewGuid(), null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    // ── TakeActionOnReportHandler ──────────────────────────────────────────

    [Fact]
    public async Task TakeAction_flips_state_without_side_effects()
    {
        using var ctx = NewDb();
        Guid lot = SeedLot(ctx);
        Guid reportId = SeedReport(ctx, lot, ReportStatus.Open, BaseTime);
        await ctx.SaveChangesAsync();

        var handler = new TakeActionOnReportHandler(ctx, Admin(), new TestClock(BaseTime.AddHours(2)));
        Result result = await handler.Handle(new TakeActionOnReportRequest(reportId, "Lot removed."), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        Report stored = await ctx.Reports.SingleAsync(r => r.Id == reportId);
        stored.Status.Should().Be(ReportStatus.ActionTaken);
        stored.ResolutionNote.Should().Be("Lot removed.");
        // The lot is untouched — actioning only records the resolution.
        Lot storedLot = await ctx.Lots.IgnoreQueryFilters().SingleAsync(l => l.Id == lot);
        storedLot.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task TakeAction_already_resolved_returns_conflict()
    {
        using var ctx = NewDb();
        Guid lot = SeedLot(ctx);
        Guid reportId = SeedReport(ctx, lot, ReportStatus.ActionTaken, BaseTime);
        await ctx.SaveChangesAsync();

        var handler = new TakeActionOnReportHandler(ctx, Admin(), new TestClock(BaseTime));
        Result result = await handler.Handle(new TakeActionOnReportRequest(reportId, "Again."), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Report.AlreadyResolved");
    }

    // ── helpers ────────────────────────────────────────────────────────────

    private static ApplicationDbContext NewDb()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private TestCurrentUser Admin() => new(_adminId, [RoleNames.Admin]);

    private static TestCurrentUser NonAdmin() => new(Guid.NewGuid(), [RoleNames.User]);

    private static Guid SeedLot(ApplicationDbContext ctx)
    {
        Guid id = Guid.NewGuid();
        ctx.Lots.Add(new Lot
        {
            Id = id,
            SellerId = Guid.NewGuid(),
            CategoryId = 1,
            Title = "Reported lot",
            Description = "Body",
            Condition = LotCondition.Ungraded,
            StartingPriceUahKopiykas = 100_00,
            CurrentPriceUahKopiykas = 100_00,
            BidCount = 0,
            Status = LotStatus.Active,
            StartsAt = BaseTime.AddHours(-2),
            EndsAt = BaseTime.AddHours(2),
            Attributes = "{}",
            CreatedAt = BaseTime.AddHours(-2),
            UpdatedAt = BaseTime.AddHours(-2),
        });
        return id;
    }

    private static Guid SeedUser(ApplicationDbContext ctx, string displayName)
    {
        Guid id = Guid.NewGuid();
        ctx.Users.Add(new User
        {
            Id = id,
            DisplayName = displayName,
            UserName = $"{displayName}@example.com",
            Email = $"{displayName}@example.com",
        });
        return id;
    }

    private static Guid SeedReport(
        ApplicationDbContext ctx,
        Guid lotId,
        ReportStatus status,
        DateTime createdAt,
        Guid? reporterUserId = null,
        string? reporterIp = null)
    {
        Guid id = Guid.NewGuid();
        ctx.Reports.Add(new Report
        {
            Id = id,
            LotId = lotId,
            ReporterUserId = reporterUserId,
            ReporterIp = reporterIp,
            Reason = ReportReason.Counterfeit,
            Note = "Looks fake",
            Status = status,
            CreatedAt = createdAt,
            ResolvedAt = status == ReportStatus.Open ? null : createdAt.AddMinutes(5),
        });
        return id;
    }

    private sealed class TestCurrentUser(Guid? userId, IReadOnlyList<string> roles) : ICurrentUserService
    {
        public Guid? UserId { get; } = userId;
        public bool IsAuthenticated => UserId.HasValue;
        public IReadOnlyList<string> Roles { get; } = roles;
    }

    private sealed class TestClock(DateTime now) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = now;
    }
}
