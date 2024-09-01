using CoinyProject.Application.Abstractions.Repositories;
using CoinyProject.Domain.Entities;
using CoinyProject.Infrastructure.Repositories.Realization;
using Microsoft.EntityFrameworkCore;

namespace CoinyProject.Infrastructure.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        private readonly ApplicationDbContext _dBContext;

        public UserRepository(ApplicationDbContext dBContext) : base(dBContext)
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
