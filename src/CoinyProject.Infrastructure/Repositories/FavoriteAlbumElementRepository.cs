using CoinyProject.Application.Abstractions.Repositories;
using CoinyProject.Domain.Entities;

namespace CoinyProject.Infrastructure.Repositories
{
    public class FavoriteAlbumElementRepository : GenericRepository<FavoriteAlbumElements>, IFavoriteAlbumElementRepository
    {
        public FavoriteAlbumElementRepository(ApplicationDbContext context) : base(context) {}

    }
}
