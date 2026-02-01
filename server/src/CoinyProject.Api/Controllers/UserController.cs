using CoinyProject.Application.Common.Querying;
using CoinyProject.Application.Common.Requests;
using CoinyProject.Application.Common.Results;
using CoinyProject.Application.Features.Albums.Models;
using CoinyProject.Application.Features.Albums.Requests;
using CoinyProject.Application.Features.Users.Models;
using CoinyProject.Application.Features.Users.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoinyProject.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UserController(IMediator mediator) : Controller
{
    [HttpPost("{id:guid}/albums/search")]
    public Task<Result<Paginated<AlbumModel>>> GetAlbumsByUser(Guid id, GetPaginatedItemsBaseRequest model, CancellationToken cancellationToken)
    {
        return mediator.Send(new GetUserAlbumsRequest(id) { Paginate = model }, cancellationToken);  
    }

    [HttpGet("{id:guid}/profile")]
    public Task<Result<UserProfileModel>> GetUserProfile(Guid id, CancellationToken cancellationToken)
    {
        return mediator.Send(new GetUserProfileRequest(id), cancellationToken);
    }
}