using CoinyProject.Domain.Entities;

namespace CoinyProject.Application.Abstractions.Repositories
{
    public interface IDiscussionTopicRepository : IBaseRepository<DiscussionTopic>
    {
        Task<IEnumerable<DiscussionTopic>> GetAllTopics();
    }
}
