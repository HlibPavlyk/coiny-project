using CoinyProject.Domain.Entities;

namespace CoinyProject.Application.Abstractions.Repositories
{
    public interface IFavoriteAlbumRepository : IBaseRepository<FavoriteAlbumElements>
    {
        Task<FavoriteAlbumElements?> GetFavoriteAlbumByAlbumId(int? id);
    }
}
