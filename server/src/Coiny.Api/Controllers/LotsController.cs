using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Requests;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Lots.Models;
using Coiny.Application.Features.Lots.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coiny.Api.Controllers;

[ApiController]
[Route("api/v1/lots")]
[Tags("Lots")]
public class LotsController(IMediator mediator) : ControllerBase
{
    /// <summary>Create a Draft lot. Requires verified email and Stripe Connect onboarding.</summary>
    [Authorize, HttpPost]
    public Task<Result<LotCreatedModel>> Create([FromBody] CreateLotRequest request, CancellationToken ct) =>
        mediator.Send(request, ct);

    /// <summary>Replace fields on a Draft lot. Owner only.</summary>
    [Authorize, HttpPut("{id:guid}")]
    public Task<Result> Update(Guid id, [FromBody] UpdateLotRequest request, CancellationToken ct) =>
        mediator.Send(request with { Id = id }, ct);

    /// <summary>Transition a Draft lot to Active and schedule the auction-close job. Owner only.</summary>
    [Authorize, HttpPost("{id:guid}/publish")]
    public Task<Result<PublishedLotModel>> Publish(Guid id, CancellationToken ct) =>
        mediator.Send(new PublishLotRequest(id), ct);

    /// <summary>
    /// Delete or cancel a lot. Behavior depends on status:
    /// Draft → hard delete + R2 cleanup. Active without bids → Cancelled. Active with bids → 409.
    /// </summary>
    [Authorize, HttpDelete("{id:guid}")]
    public Task<Result> Delete(Guid id, CancellationToken ct) =>
        mediator.Send(new DeleteLotRequest(id), ct);

    /// <summary>Public lot detail with images, breadcrumb, seller and (if closed) winning bid.</summary>
    [HttpGet("{id:guid}")]
    public Task<Result<LotDetailModel>> Get(Guid id, CancellationToken ct) =>
        mediator.Send(new GetLotByIdRequest(id), ct);

    /// <summary>
    /// Public, paginated lot search over the whole collection. Filter by category (and its leaf
    /// descendants), seller, and/or published status via the request body. Only Active/Sold lots are
    /// ever returned — seller-private states live behind <c>mine/search</c>.
    /// </summary>
    [HttpPost("search")]
    public Task<Result<Paginated<LotCardModel>>> Search([FromBody] GetPublicLotsRequest request, CancellationToken ct) =>
        mediator.Send(request, ct);

    /// <summary>Authenticated caller's own lots in any status (soft-deleted included with deletedAt).</summary>
    [Authorize, HttpPost("mine/search")]
    public Task<Result<Paginated<MyLotItemModel>>> SearchMine([FromBody] GetMyLotsRequest request, CancellationToken ct) =>
        mediator.Send(request, ct);

    /// <summary>Upload a single image to a Draft lot (max 5 per lot, max 10 MB, JPEG/PNG/WebP).</summary>
    [Authorize, Consumes("multipart/form-data"), HttpPost("{id:guid}/images")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<Result<LotImageUploadModel>> UploadImage(Guid id, IFormFile file, CancellationToken ct)
    {
        await using Stream stream = file.OpenReadStream();
        return await mediator.Send(new UploadLotImageRequest(id, stream, file.ContentType, file.Length), ct);
    }

    /// <summary>Remove an image from a Draft lot (R2 + DB) and re-pack remaining display orders.</summary>
    [Authorize ,HttpDelete("{id:guid}/images/{imageId:guid}")]
    public Task<Result> DeleteImage(Guid id, Guid imageId, CancellationToken ct) =>
        mediator.Send(new DeleteLotImageRequest(id, imageId), ct);

    /// <summary>Reorder a Draft lot's images. Body must contain the exact existing image IDs in desired order.</summary>
    [Authorize, HttpPost("{id:guid}/images/reorder")]
    public Task<Result> ReorderImages(Guid id, [FromBody] ReorderLotImagesRequest request, CancellationToken ct) =>
        mediator.Send(request with { LotId = id }, ct);

    /// <summary>File a report on a lot. Anonymous allowed; rate-limited (3/hour anon, 5/hour authenticated).</summary>
    [Tags("Reports")]
    [HttpPost("{id:guid}/report")]
    public Task<Result> Report(Guid id, [FromBody] ReportLotRequest request, CancellationToken ct) =>
        mediator.Send(request with { LotId = id }, ct);
}
