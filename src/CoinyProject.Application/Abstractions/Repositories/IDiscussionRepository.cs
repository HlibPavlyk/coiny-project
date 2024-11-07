using CoinyProject.Application.Dto.Other;
using CoinyProject.Domain.Entities;

namespace CoinyProject.Application.Abstractions.Repositories
{
    public interface IDiscussionRepository : IGenericRepository<Discussion>
    {
        Task<PagedResponse<Discussion>> GetPagedDiscussionsWithUserAsync(int page, int size);
        Task<Discussion?> GetDiscussionWithUserAndMessagesByIdAsync(Guid id);
    }
}
