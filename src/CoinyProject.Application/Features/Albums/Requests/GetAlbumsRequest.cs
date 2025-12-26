using CoinyProject.Application.Common.Querying;
using CoinyProject.Application.Common.Requests;
using CoinyProject.Application.Common.Results;
using CoinyProject.Application.Features.Albums.Models;
using MediatR;

namespace CoinyProject.Application.Features.Albums.Requests;

public record GetAlbumsRequest : GetPaginatedItemsBaseRequest, IRequest<Result<Paginated<AlbumModel>>>;