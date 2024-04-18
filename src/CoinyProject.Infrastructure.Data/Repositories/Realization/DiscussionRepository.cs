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
    public class DiscussionRepository : BaseRepository<Discussion>, IDiscussionRepository
    {
        private readonly ApplicationDBContext _dBContext;

        public DiscussionRepository(ApplicationDBContext dBContext) : base(dBContext)
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
