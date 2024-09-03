using CoinyProject.Application.Abstractions.Repositories;
using CoinyProject.Application.Dto.Other;
using CoinyProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoinyProject.Infrastructure.Repositories
{
    public class DiscussionRepository : GenericRepository<Discussion>, IDiscussionRepository
    {
        public DiscussionRepository(ApplicationDbContext context) : base(context) {}

        public async Task<PagedResponse<Discussion>> GetPagedDiscussionsWithUserAsync(int page, int size)
        {
            var query = Context.Discussions
                .AsNoTracking()
                .Include(d => d.User);
            
            return await GetPagedEntitiesAsync(query, page, size);
        }

        public async Task<Discussion?> GetDiscussionWithUserAndMessagesByIdAsync(Guid id)
        {
            return await Context.Discussions
                .AsNoTracking()
                .Include(d => d.User)
                .SingleOrDefaultAsync(d => d.Id == id);
        }
    }
}
