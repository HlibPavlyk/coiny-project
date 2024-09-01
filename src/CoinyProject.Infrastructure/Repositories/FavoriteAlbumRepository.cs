using CoinyProject.Application.Abstractions.Repositories;
using CoinyProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoinyProject.Infrastructure.Repositories
{
    public class FavoriteAlbumRepository : BaseRepository<FavoriteAlbums>, IFavoriteAlbumRepository
    {
        private readonly ApplicationDbContext _dBContext;

        public FavoriteAlbumRepository(ApplicationDbContext dBContext) : base(dBContext)
        {
            _dBContext = dBContext;
        }

        public async Task<FavoriteAlbums?> GetFavoriteAlbumByAlbumId(int? id)
        {
            return await _dBContext.FavoriteAlbums
                .Where(u => u.Id == id)
                .FirstOrDefaultAsync();
        }
    }
}
