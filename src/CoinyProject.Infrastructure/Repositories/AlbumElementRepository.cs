using CoinyProject.Application.Abstractions.Repositories;
using CoinyProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoinyProject.Infrastructure.Repositories
{
    public class AlbumElementRepository : BaseRepository<AlbumElement>, IAlbumElementRepository
    {
        private readonly ApplicationDbContext _dBContext;

        public AlbumElementRepository(ApplicationDbContext dBContext) : base(dBContext)
        {
            _dBContext = dBContext;
        }

        public async Task<AlbumElement?> GetAlbumElementById(Guid? id)
        {
            return await _dBContext.AlbumElements
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<AlbumElement?> GetAlbumElementWithAuthorCheck(int? id, string? currentUserId)
        {
            return await _dBContext.AlbumElements
                .Include(x => x.Album)
                .Where(x => x.Id == id)
                .Where(x => x.Album.UserId == currentUserId)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }
    }
}
