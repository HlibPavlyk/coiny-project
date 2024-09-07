using CoinyProject.Application.Dto.Other;
using CoinyProject.Domain.Entities;

namespace CoinyProject.Application.Abstractions.Repositories
{
    public interface IAlbumRepository : IGenericRepository<Album>
    {
        Task<PagedResponse<Album>> GetPagedActiveAlbumsWithElementsAsync(int page, int size);
        Task<PagedResponse<Album>> GetPagedAlbumsWithElementsAndFavoritesForViewAsync(int page, int size);
        //Task<Album?> GetAlbumWithElementsByIdAsync(Guid id);
    }
}
