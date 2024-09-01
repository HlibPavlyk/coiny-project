using CoinyProject.Domain.Entities;

namespace CoinyProject.Application.Abstractions.Repositories
{
    public interface IAlbumRepository : IBaseRepository<Album>
    {
        Task<IEnumerable<Album>?> GetAllAlbumsWithElements(string? userId);
        Task<IEnumerable<Album>?> GetAllAlbumsWithElementsAndFavoritesForView();
        Task<Album?> GetAlbumWithElementsById(int? id);
        Task<Album?> GetAlbumWithAuthorCheck(int? id, string? currentUserId);
        Task<Album?> GetAlbumById(int? id);
    }
}
