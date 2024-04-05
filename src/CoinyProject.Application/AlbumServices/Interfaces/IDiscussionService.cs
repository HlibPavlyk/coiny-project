using CoinyProject.Application.DTO.Album;
using CoinyProject.Application.DTO.Discussion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Application.AlbumServices.Interfaces
{
    public interface IDiscussionService
    {
        Task AddDiscussion(DiscussionCreateDTO discussion, string userId);
        Task<IEnumerable<DiscussionTopicDTO>> GetAvailableTopics();
        Task<IEnumerable<DiscussionGetForViewDTO>> GetAllDiscussionsForView();
        Task AddDiscussionMessage(DiscussionMessageCreateDTO message);
        Task<DiscussionGetByIdDTO> GetDiscussionById(int discussionId);
    }
}
