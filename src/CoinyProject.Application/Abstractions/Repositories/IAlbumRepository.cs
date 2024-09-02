using CoinyProject.Domain.Entities;

namespace CoinyProject.Application.Abstractions.Repositories
{
    public interface IAlbumRepository : IGenericRepository<Album>
    {
        Task<IEnumerable<Album>?> GetAllAlbumsWithElementsByUserId(Guid id);
        Task<IEnumerable<Album>?> GetAllAlbumsWithElementsAndFavoritesForView();
        Task<Album?> GetAlbumWithElementsById(Guid id);
        Task<Album?> GetAlbumWithAuthorCheck(Guid id, Guid currentUserId);
    }
}
