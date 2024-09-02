using CoinyProject.Application.Abstractions.Repositories;
using CoinyProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoinyProject.Infrastructure.Repositories
{
    public class AlbumElementRepository : GenericRepository<AlbumElement>, IAlbumElementRepository
    {
        public AlbumElementRepository(ApplicationDbContext context) : base(context) {}

        public async Task<AlbumElement?> GetAlbumElementById(Guid id)
        {
            return await _context.AlbumElements
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<AlbumElement?> GetAlbumElementWithAuthorCheck(Guid id, Guid currentUserId)
        {
            return await _context.AlbumElements
                .Include(x => x.Album)
                .Where(x => x.Id == id)
                .Where(x => x.Album.UserId == currentUserId)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }
    }
}
