using CoinyProject.Application.Common.Querying;
using CoinyProject.Application.Common.Results;
using CoinyProject.Application.Features.Albums.Models;
using CoinyProject.Application.Features.Albums.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoinyProject.Api.Controllers;

[ApiController]
[Route("api/v1/albums")]
public class AlbumController(IMediator mediator) : Controller
{
    [HttpPost("search")]
    public Task<Result<Paginated<AlbumModel>>> GetAlbums(GetAlbumsRequest request, CancellationToken cancellationToken)
    {
        return mediator.Send(request, cancellationToken);
    }

    [HttpGet("{id:guid}")]
    public Task<Result<AlbumModel>> GetAlbumById(Guid id, CancellationToken cancellationToken)
    {
        return mediator.Send(new GetAlbumByIdRequest(id), cancellationToken);
    }
    
    [Authorize, HttpPost]
    public Task<Result<Guid>> AddAlbum(CreateAlbumRequest request, CancellationToken cancellationToken)
    { 
        return mediator.Send(request, cancellationToken);
    }

    [Authorize, HttpPatch("{id:guid}")]
    public Task<Result<Guid>> UpdateAlbum(Guid id, UpdateAlbumModel model, CancellationToken cancellationToken)
    {
        return mediator.Send(new UpdateAlbumRequest(id, model), cancellationToken);
    }

    [Authorize, HttpPost("{id:guid}/deactivate")]
    public Task<Result<Guid>> DeactivateAlbum(Guid id, CancellationToken cancellationToken)
    {
        return mediator.Send(new DeactivateAlbumRequest(id), cancellationToken);
    }

    [Authorize, HttpPost("{id:guid}/activate")]
    public Task<Result<Guid>> ActivateAlbum(Guid id, CancellationToken cancellationToken)
    {
        return mediator.Send(new ActivateAlbumRequest(id), cancellationToken);
    }
}
