using Coiny.Application.Common.Results;
using Coiny.Application.Features.Categories.GetCategoryTree;
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
}
