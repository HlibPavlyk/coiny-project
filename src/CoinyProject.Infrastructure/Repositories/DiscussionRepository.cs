using CoinyProject.Application.Abstractions.Repositories;
using CoinyProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoinyProject.Infrastructure.Repositories
{
    public class DiscussionRepository : GenericRepository<Discussion>, IDiscussionRepository
    {
        public DiscussionRepository(ApplicationDbContext context) : base(context) {}

        public async Task<IEnumerable<Discussion>> GetAllDiscussionsWithUser()
        {
            return await _context.Discussions
                .Include(x => x.User)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Discussion?> GetDiscussionWithUserAndMessagesById(Guid id)
        {
            return await _context.Discussions
               .Include(x => x.User)
               .Include(x => x.Messages)
                    .ThenInclude(x => x.User)
               .Where(x => x.Id == id)
               .AsNoTracking()
               .FirstOrDefaultAsync();
        }


    }
}
