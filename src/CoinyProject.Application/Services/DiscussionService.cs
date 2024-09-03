using AutoMapper;
using CoinyProject.Application.Abstractions.Repositories;
using CoinyProject.Application.AlbumServices.Interfaces;
using CoinyProject.Application.DTO.Discussion;
using CoinyProject.Domain.Entities;

namespace CoinyProject.Application.Services
{
    public class DiscussionService :IDiscussionService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public DiscussionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task AddDiscussion(DiscussionCreateDTO? discussion, string? userId)
        {
            if(discussion == null || userId == null)
                throw new ArgumentNullException(nameof(discussion), "discussion or userId is null");

            var _discussion = _mapper.Map<Discussion>(discussion);
            _discussion.UserId = userId;

            await _unitOfWork.Discussions.InsertAsync(_discussion);
            await _unitOfWork.SaveChangesAsync();

        }

        public async Task<IEnumerable<DiscussionTopicDTO>> GetAvailableTopics()
        {
            var topics = await _unitOfWork.DiscussionTopics.GetAllTopics();

            if(topics == null)
                throw new ArgumentNullException("topics is null");

            return _mapper.Map<List<DiscussionTopicDTO>>(topics);
        }

        public async Task<IEnumerable<DiscussionGetForViewDTO>> GetAllDiscussionsForView()
        {
            var _discussions = await _unitOfWork.Discussions.GetAllDiscussionsWithUserAndTopic();
            
            if(_discussions == null)
                throw new ArgumentNullException("discussions is null");

            return _mapper.Map<List<DiscussionGetForViewDTO>>(_discussions);
        }

        public async Task AddDiscussionMessage(DiscussionMessageCreateDTO? message)
        {
            if(message == null)
                throw new ArgumentNullException("message is null");

            var _message = _mapper.Map<DiscussionMessage>(message);

            if(_message != null)
            {
                await _unitOfWork.DiscussionMessages.InsertAsync(_message);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<DiscussionGetByIdDTO> GetDiscussionById(int? discussionId)
        {
            if (discussionId == null)
                throw new ArgumentNullException("discussionId is null");

            var _discussion = await _unitOfWork.Discussions.GetDiscussionWithUserAndTopicAndMessagesById(discussionId);

            if(_discussion == null)
                throw new ArgumentNullException("discussion is null");
            
            return _mapper.Map<DiscussionGetByIdDTO>(_discussion);
        }

    }
}
