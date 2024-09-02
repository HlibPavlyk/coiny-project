using CoinyProject.Application.Abstractions.Repositories;
using CoinyProject.Domain.Entities;

namespace CoinyProject.Infrastructure.Repositories
{
    public class DiscussionMessageRepository : GenericRepository<DiscussionMessage>, IDiscussionMessageRepository
    {
        public DiscussionMessageRepository(ApplicationDbContext context) : base(context) {}
    }
}
