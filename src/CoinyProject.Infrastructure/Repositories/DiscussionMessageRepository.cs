using CoinyProject.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoinyProject.Application.Abstractions.Repositories;

namespace CoinyProject.Infrastructure.Repositories.Realization
{
    public class DiscussionMessageRepository : BaseRepository<DiscussionMessage>, IDiscussionMessageRepository
    {
        private readonly ApplicationDbContext _dBContext;

        public DiscussionMessageRepository(ApplicationDbContext dBContext) : base(dBContext)
        {
            _dBContext = dBContext;
        }
    }
}
