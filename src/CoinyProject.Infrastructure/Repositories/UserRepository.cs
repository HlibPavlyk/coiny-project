using CoinyProject.Application.Abstractions.Repositories;
using CoinyProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoinyProject.Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context) {}

        public async Task<User?> GetUserWithInfoForStatsAsync(Guid id)
        {
            return await Context.Users
                .AsNoTracking()
                .Include(u => u.Albums)
                .ThenInclude(a => a.Elements)
                .FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}
