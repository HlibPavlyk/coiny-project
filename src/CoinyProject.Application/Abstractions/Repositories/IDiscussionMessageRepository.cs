using CoinyProject.Application.Dto.Other;
using CoinyProject.Domain.Entities;

namespace CoinyProject.Application.Abstractions.Repositories
{
    public interface IDiscussionMessageRepository : IGenericRepository<DiscussionMessage>
    {
        Task<PagedResponse<DiscussionMessage>> GetPagedDiscussionMessagesWithUserByDiscussionIdAsync(
            Guid id, int page, int size);
    }
}
