using CoinyProject.Application.DTO.Discussion;

namespace CoinyProject.Application.Abstractions.Services
{
    public interface IDiscussionService
    {
        Task AddDiscussion(DiscussionCreateDTO? discussion, string? userId);
        Task<IEnumerable<DiscussionTopicDTO>> GetAvailableTopics();
        Task<IEnumerable<DiscussionGetForViewDTO>> GetAllDiscussionsForView();
        Task AddDiscussionMessage(DiscussionMessageCreateDTO? message);
        Task<DiscussionGetByIdDTO> GetDiscussionById(int? discussionId);
    }
}
