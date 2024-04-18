using CoinyProject.Core.Domain.Entities;
using CoinyProject.Infrastructure.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Infrastructure.Data.Repositories.Realization
{
    public class FavoriteAlbumRepository : BaseRepository<FavoriteAlbums>, IFavoriteAlbumRepository
    {
        private readonly ApplicationDBContext _dBContext;

        public FavoriteAlbumRepository(ApplicationDBContext dBContext) : base(dBContext)
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
