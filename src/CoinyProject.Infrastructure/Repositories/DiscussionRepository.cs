using CoinyProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoinyProject.Application.Abstractions.Repositories;

namespace CoinyProject.Infrastructure.Repositories.Realization
{
    public class DiscussionRepository : BaseRepository<Discussion>, IDiscussionRepository
    {
        private readonly ApplicationDbContext _dBContext;

        public DiscussionRepository(ApplicationDbContext dBContext) : base(dBContext)
        {
            _dBContext = dBContext;
        }

        public async Task<IEnumerable<Discussion>> GetAllDiscussionsWithUserAndTopic()
        {
            return await _dBContext.Discussions
                .Include(x => x.User)
                .Include(x => x.DiscussionTopic)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Discussion> GetDiscussionWithUserAndTopicAndMessagesById(int? discussionId)
        {
            return await _dBContext.Discussions
               .Include(x => x.User)
               .Include(x => x.DiscussionTopic)
               .Include(x => x.Messages)
               .ThenInclude(x => x.User)
               .Where(x => x.Id == discussionId)
               .AsNoTracking()
               .FirstOrDefaultAsync();
        }


    }
}
