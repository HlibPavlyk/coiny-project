using Coiny.Application.Abstractions.Data;
using Coiny.Application.Abstractions.Http;
using Coiny.Application.Common.Authorization;
using Coiny.Application.Common.Extensions;
using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Admin.Models;
using Coiny.Application.Features.Admin.Requests;
using Coiny.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coiny.Application.Features.Admin.Handlers;

/// <summary>
/// Paginated admin reports listing, filtered by status and ordered by the requested column (default
/// newest-first). Role is enforced at the controller via <c>[Authorize(Roles = "Admin")]</c>; the
/// handler re-checks the caller's roles as defense-in-depth. Query filters are ignored so a report on
/// an already-soft-deleted lot still surfaces the lot it targeted.
/// </summary>
public class GetReportsHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<GetReportsRequest, Result<Paginated<ReportItemModel>>>
{
    public async Task<Result<Paginated<ReportItemModel>>> Handle(GetReportsRequest request, CancellationToken ct)
    {
        if (!currentUser.CanModerate())
            return Result.Failure<Paginated<ReportItemModel>>(
                Error.Forbidden("Admin.Forbidden", "Moderator or Administrator role required."));

        IReadOnlyList<SortByModel> sort = request.SortBy is { Length: > 0 }
            ? request.SortBy
            : [new SortByModel("createdAt", SortDirection.Desc)];

        Result<IQueryable<Report>> sortResult = db.Reports
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(r => request.Filters.Status == null || r.Status == request.Filters.Status)
            .SortBy(sort, r => r.CreatedAt, r => r.ResolvedAt!);

        if (sortResult.IsFailure)
            return Result.Failure<Paginated<ReportItemModel>>(sortResult.Error);

        int totalCount = await sortResult.Value.CountAsync(ct);

        List<ReportItemModel> items = await sortResult.Value
            .Paginate(request)
            .Select(r => new ReportItemModel(
                r.Id,
                new ReportLotInfo(
                    r.Lot!.Id,
                    r.Lot.Title,
                    r.Lot.Images.OrderBy(i => i.DisplayOrder).Select(i => i.PublicUrl).FirstOrDefault() ?? string.Empty),
                r.Reporter != null ? r.Reporter.DisplayName : null,
                r.ReporterIp,
                r.Reason,
                r.Note,
                r.Status,
                r.CreatedAt,
                r.ResolvedAt))
            .ToListAsync(ct);

        return Result.Success(new Paginated<ReportItemModel>(totalCount, items));
    }
}
