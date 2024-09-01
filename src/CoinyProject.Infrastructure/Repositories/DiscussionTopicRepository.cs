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
    public class DiscussionTopicRepository : BaseRepository<DiscussionTopic>, IDiscussionTopicRepository
    {
        private readonly ApplicationDbContext _dBContext;

        public DiscussionTopicRepository(ApplicationDbContext dBContext) : base(dBContext)
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
