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
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        private readonly ApplicationDBContext _dBContext;

        public UserRepository(ApplicationDBContext dBContext) : base(dBContext)
        {
            _dBContext = dBContext;
        }

        public async Task<User?> GetUserWithFavoriteAlbumsById(string? id)
        {
            return await _dBContext.Users
                .Include(u => u.FavoriteAlbums)
                .Where(u => u.Id == id)
                .FirstOrDefaultAsync();
        }
    }
}
