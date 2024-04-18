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
    public class DiscussionTopicRepository : BaseRepository<DiscussionTopic>, IDiscussionTopicRepository
    {
        private readonly ApplicationDBContext _dBContext;

        public DiscussionTopicRepository(ApplicationDBContext dBContext) : base(dBContext)
        {
            _dBContext = dBContext;
        }

        public async Task<IEnumerable<DiscussionTopic>> GetAllTopics()
        {
            return await _dBContext.DiscussionTopics
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
