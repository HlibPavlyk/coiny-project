using CoinyProject.Application.Abstractions.Repositories;
using CoinyProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoinyProject.Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context) {}

        public async Task<User?> GetUserWithFavoriteAlbumElementsById(Guid id)
        {
            return await _context.Users
                .Include(u => u.FavoriteAlbumElements)
                .Where(u => u.Id == id)
                .FirstOrDefaultAsync();
        }
    }
}
