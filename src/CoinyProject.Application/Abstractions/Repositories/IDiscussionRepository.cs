using CoinyProject.Domain.Entities;

namespace CoinyProject.Application.Abstractions.Repositories
{
    public interface IDiscussionRepository : IGenericRepository<Discussion>
    {
        Task<IEnumerable<Discussion>> GetAllDiscussionsWithUser();
        Task<Discussion?> GetDiscussionWithUserAndMessagesById(Guid id);
    }
}
