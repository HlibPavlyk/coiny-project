using CoinyProject.Domain.Entities;

namespace CoinyProject.Application.Abstractions.Repositories
{
    public interface IDiscussionRepository : IBaseRepository<Discussion>
    {
        Task<IEnumerable<Discussion>> GetAllDiscussionsWithUserAndTopic();
        Task<Discussion> GetDiscussionWithUserAndTopicAndMessagesById(int? discussionId);
    }
}
