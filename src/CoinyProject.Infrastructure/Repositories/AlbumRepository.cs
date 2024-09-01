using CoinyProject.Domain.Entities;
using CoinyProject.Infrastructure.Repositories.Realization;
using Microsoft.EntityFrameworkCore;

namespace CoinyProject.Infrastructure.Repositories
{
    public class AlbumRepository : BaseRepository<Album>, IAlbumRepository
    {
        private readonly ApplicationDbContext _dBContext;

        public AlbumRepository(ApplicationDbContext dBContext) : base(dBContext)
        {
            _dBContext = dBContext;
        }


        public async Task<Album?> GetAlbumWithElementsById(int? id)
        {
            return await _dBContext.Albums
                .Where(x => x.Id == id)
                .Include(x => x.Elements)
                .FirstOrDefaultAsync();
        }

        public async Task<Album?> GetAlbumWithAuthorCheck(int? id, string? currentUserId)
        {
            return await _dBContext.Albums
                .Where(x => x.Id == id)
                .Where(x => x.UserId == currentUserId)
                .AsNoTracking()
                .FirstOrDefaultAsync();

        }

        public async Task<IEnumerable<Album>?> GetAllAlbumsWithElements(string? userId)
        {
            return await _dBContext.Albums
                .Include(x => x.Elements)
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Album>?> GetAllAlbumsWithElementsAndFavoritesForView()
        {
            return await _dBContext.Albums
                .Include(x => x.Elements)
                .Include(x => x.FavoriteAlbums)
                .Where(x => x.Elements.Count > 0)
                .OrderByDescending(x => x.Rate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Album?> GetAlbumById(int? id)
        {
            return await _dBContext.Albums
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();
        }
    }
}
