using CoinyProject.Application.Abstractions.Repositories;
using CoinyProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoinyProject.Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context) {}

        public async Task<User?> GetUserWithFavoriteAlbumElementsByIdAsync(Guid id)
        {
            return await Context.Users
                .AsNoTracking()
                .Include(u => u.FavoriteAlbumElements)
                .SingleOrDefaultAsync(u => u.Id == id);
        }
    }
}
