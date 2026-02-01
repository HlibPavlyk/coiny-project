using CoinyProject.Application.Common.Querying;
using CoinyProject.Application.Common.Requests;
using CoinyProject.Application.Common.Results;
using CoinyProject.Application.Features.AlbumElements.Models;
using MediatR;

namespace CoinyProject.Application.Features.AlbumElements.Requests;

public record GetAlbumElementsRequest(Guid AlbumId): GetPaginatedItemsByModelBaseRequest, IRequest<Result<Paginated<AlbumElementListItemModel>>>;
