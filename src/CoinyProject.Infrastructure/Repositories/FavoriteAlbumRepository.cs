using CoinyProject.Application.Abstractions.Repositories;
using CoinyProject.Domain.Entities;

namespace CoinyProject.Infrastructure.Repositories
{
    public class FavoriteAlbumRepository : GenericRepository<FavoriteAlbumElements>, IFavoriteAlbumRepository
    {
        public FavoriteAlbumRepository(ApplicationDbContext context) : base(context) {}

    }
}
