using CoinyProject.Application.DTO.Album;
using CoinyProject.Application.Models;
using MediatR;

namespace CoinyProject.Application.Requests.Albums;

public record GetAlbumItemsRequest : GetPaginatedItemsBaseRequest, IRequest<PaginatedItemsModel<AlbumViewGetDto>>;