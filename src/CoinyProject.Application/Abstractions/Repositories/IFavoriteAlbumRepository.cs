using CoinyProject.Domain.Entities;

namespace CoinyProject.Application.Abstractions.Repositories
{
    public interface IFavoriteAlbumRepository : IBaseRepository<FavoriteAlbums>
    {
        Task<FavoriteAlbums?> GetFavoriteAlbumByAlbumId(int? id);
    }
}
