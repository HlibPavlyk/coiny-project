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
    public class AlbumElementRepository : BaseRepository<AlbumElement>, IAlbumElementRepository
    {
        private readonly ApplicationDBContext _dBContext;

        public AlbumElementRepository(ApplicationDBContext dBContext) : base(dBContext)
        {
            _dBContext = dBContext;
        }

        public async Task<AlbumElement?> GetAlbumElementById(int? id)
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
