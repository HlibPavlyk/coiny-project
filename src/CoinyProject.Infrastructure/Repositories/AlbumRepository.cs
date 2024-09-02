using CoinyProject.Application.Abstractions.Repositories;
using CoinyProject.Domain.Entities;
using CoinyProject.Infrastructure.Repositories.Realization;
using Microsoft.EntityFrameworkCore;

namespace CoinyProject.Infrastructure.Repositories
{
    public class AlbumRepository : GenericRepository<Album>, IAlbumRepository
    {
        public AlbumRepository(ApplicationDbContext context) : base(context) {}


        public async Task<Album?> GetAlbumWithElementsById(Guid id)
        {
            return await _context.Albums
                .Include(x => x.Elements)
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Album?> GetAlbumWithAuthorCheck(Guid id, Guid currentUserId)
        {
            return await _context.Albums
                .Where(x => x.Id == id)
                .Where(x => x.UserId == currentUserId)
                .AsNoTracking()
                .FirstOrDefaultAsync();

        }

        public async Task<IEnumerable<Album>?> GetAllAlbumsWithElementsByUserId(Guid id)
        {
            return await _context.Albums
                .Include(x => x.Elements)
                .AsNoTracking()
                .Where(x => x.UserId == id)
                .ToListAsync();
        }

        public async Task<IEnumerable<Album>?> GetAllAlbumsWithElementsAndFavoritesForView()
        {
            return await _context.Albums
                .Include(x => x.Elements)
                    .ThenInclude(x => x.FavoriteAlbumElements)
                .Where(x => x.Elements.Count > 0)
                .OrderByDescending(x => x.Rate)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
