using CoinyProject.Application.Abstractions.Repositories;
using CoinyProject.Application.Dto.Other;
using CoinyProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoinyProject.Infrastructure.Repositories
{
    public class DiscussionMessageRepository : GenericRepository<DiscussionMessage>, IDiscussionMessageRepository
    {
        public DiscussionMessageRepository(ApplicationDbContext context) : base(context) {}


        public async Task<PagedResponse<DiscussionMessage>> GetPagedDiscussionMessagesWithUserByDiscussionIdAsync(
            Guid id, int page, int size)
        {
            var query = Context.DiscussionMessages
                .AsNoTracking()
                .Include(d => d.User)
                .Where(d => d.DiscussionId == id);
            
            return await GetPagedEntitiesAsync(query, page, size);
        }
    }
}
