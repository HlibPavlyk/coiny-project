using Coiny.Application.Common.Querying;
using Coiny.Application.Common.Requests;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Categories.Models;
using Coiny.Application.Features.Categories.Requests;
using Coiny.Application.Features.Lots.Models;
using Coiny.Application.Features.Lots.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Coiny.Api.Controllers;

[ApiController]
[Route("api/v1/categories")]
[Tags("Categories")]
public class CategoriesController(IMediator mediator) : ControllerBase
{
    /// <summary>Returns the full hardcoded category tree with leaf-level active lot counts. Cached server-side for 5 minutes.</summary>
    [HttpGet]
    public Task<Result<CategoryTreeModel>> GetTree(CancellationToken ct) =>
        mediator.Send(new GetCategoryTreeRequest(), ct);

    /// <summary>Active lots in a category (or all its leaf descendants), paginated.</summary>
    [HttpPost("{categoryId:int}/lots/search")]
    public Task<Result<Paginated<LotCardModel>>> SearchLots(
        int categoryId,
        [FromBody] PageRequest paginate,
        CancellationToken ct) =>
        mediator.Send(new GetLotsByCategoryRequest { CategoryId = categoryId, Paginate = paginate }, ct);
}
