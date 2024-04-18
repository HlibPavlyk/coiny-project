using CoinyProject.Core.Domain.Entities;
using CoinyProject.Infrastructure.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CoinyProject.Infrastructure.Data.Repositories.Realization
{
    public class AlbumRepository : BaseRepository<Album>, IAlbumRepository
    {
        private readonly ApplicationDBContext _dBContext;

        public AlbumRepository(ApplicationDBContext dBContext) : base(dBContext)
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

        public async Task<IEnumerable<Album>?> GetAllAlbumsWithElementsAndFavoritesForView(string? userId)
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
