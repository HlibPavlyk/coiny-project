using AutoMapper;
using CoinyProject.Application.Abstractions.Repositories;
using CoinyProject.Application.Common.Extensions;
using CoinyProject.Application.Common.Querying.Models;
using CoinyProject.Application.Features.Albums.Models;
using CoinyProject.Application.Features.Albums.Requests;
using MediatR;

namespace CoinyProject.Application.Features.Albums.Handlers;

public class AlbumsHandler(IUnitOfWork unitOfWork, IMapper mapper) :
    IRequestHandler<GetAlbumItemsRequest, PaginatedItemsModel<AlbumViewGetModel>>
{
    public async Task<PaginatedItemsModel<AlbumViewGetModel>> Handle(GetAlbumItemsRequest request, CancellationToken cancellationToken)
    {
        var items = await GetAlbumItemsAsync(request, request.SortBy, cancellationToken);

        return new PaginatedItemsModel<AlbumViewGetModel>
        {
            TotalCount = items.Length,
            Items = items.Paginate(request).ToArray(),
        };
    }
    
    private async Task<AlbumViewGetModel[]> GetAlbumItemsAsync(
        ITextSearch search,
        SortByModel[] sortBy,
        CancellationToken cancellationToken)
    { 
        var items = await unitOfWork.Albums.GetAllAsyncEnumerable()
            //.Where(x => x.Status == AlbumStatus.Active)
            .Search(
                search, 
                position => position.Name,
                position => position.Status,
                position => position.Description,
                position => position.Rate,
                position => position.CreatedAt,
                position => position.UpdatedAt
            )
            .Select(mapper.Map<AlbumViewGetModel>)
            .ToArrayAsync(cancellationToken);

        return items.SortBy(sortBy).ToArray();
    }
}