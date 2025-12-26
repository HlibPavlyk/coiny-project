using CoinyProject.Application.Common.Models;
using CoinyProject.Application.Common.Querying;
using CoinyProject.Application.Common.Requests;
using CoinyProject.Application.Common.Results;
using CoinyProject.Application.Features.AlbumElements.Models;
using CoinyProject.Application.Features.AlbumElements.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoinyProject.Api.Controllers;

[ApiController]
[Route("api/v1/albums/{albumId:guid}/elements")]
public class AlbumElementController(IMediator mediator) : Controller
{
    [HttpPost("search")]
    public Task<Result<Paginated<AlbumElementListItemModel>>> GetAlbumElements(Guid albumId, GetPaginatedItemsBaseRequest model, CancellationToken cancellationToken)
    {
        return mediator.Send(new GetAlbumElementsRequest(albumId) { Paginate = model }, cancellationToken);  
    }
    
    [HttpGet("{id:guid}")]
    public Task<Result<AlbumElementModel>> GetAlbumElementById(Guid id, Guid albumId, CancellationToken cancellationToken)
    {
        return mediator.Send(new GetAlbumElementByIdRequest(id, albumId), cancellationToken);
    }
    
    [Authorize, HttpPost]
    public Task<Result<Guid>> AddAlbumElement(Guid albumId, [FromForm] UpdateAlbumElementModel model, IFormFile file, CancellationToken cancellationToken)
    {
        return mediator.Send(new AddAlbumElementRequest(
            albumId, model, new FileStreamDataModel(file.FileName, file.OpenReadStream())), cancellationToken);
    }
    
    [Authorize, HttpPatch("{id:guid}")]
    public Task<Result<Guid>> UpdateAlbumElement(Guid id, Guid albumId, [FromForm] UpdateAlbumElementModel model, IFormFile file, CancellationToken cancellationToken)
    {
        return mediator.Send(new UpdateAlbumElementRequest(
            id, albumId, model, new FileStreamDataModel(file.FileName, file.OpenReadStream())), cancellationToken);
    }
    
    [Authorize, HttpDelete("{id:guid}")]
    public Task<Result<Guid>> DeleteAlbumElement(Guid id, Guid albumId, CancellationToken cancellationToken)
    {
        return mediator.Send(new DeleteAlbumElementRequest(id, albumId), cancellationToken);
    }
}
