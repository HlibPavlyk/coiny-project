using CoinyProject.Application.Common.Querying.Models;
using CoinyProject.Application.Common.Requests;
using CoinyProject.Application.Features.Albums.Models;
using MediatR;

namespace CoinyProject.Application.Features.Albums.Requests;

public record GetAlbumItemsRequest : GetPaginatedItemsBaseRequest, IRequest<PaginatedItemsModel<AlbumViewGetModel>>;