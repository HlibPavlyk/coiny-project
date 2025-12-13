using AutoMapper;
using CoinyProject.Application.Abstractions.Repositories;
using CoinyProject.Application.Common.Extensions;
using CoinyProject.Application.DTO.Album;
using CoinyProject.Application.Models;
using CoinyProject.Application.Requests.Albums;
using CoinyProject.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Hosting.Internal;

namespace CoinyProject.Application.Handlers.Albums;

public class AlbumsHandler(IUnitOfWork unitOfWork, IMapper mapper) :
    IRequestHandler<GetAlbumItemsRequest, PaginatedItemsModel<AlbumViewGetDto>>
{
    public async Task<PaginatedItemsModel<AlbumViewGetDto>> Handle(GetAlbumItemsRequest request, CancellationToken cancellationToken)
    {
        var items = await GetAlbumItemsAsync(request, request.SortBy, cancellationToken);

        return new PaginatedItemsModel<AlbumViewGetDto>
        {
            TotalCount = items.Length,
            Items = items.Paginate(request).ToArray(),
        };
    }
    
    private async Task<AlbumViewGetDto[]> GetAlbumItemsAsync(
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
            .Select(mapper.Map<AlbumViewGetDto>)
            .ToArrayAsync(cancellationToken);

        return items.SortBy(sortBy).ToArray();
    }
}