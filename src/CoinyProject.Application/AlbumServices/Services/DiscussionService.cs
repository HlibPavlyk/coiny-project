using AutoMapper;
using CoinyProject.Application.AlbumServices.Interfaces;
using CoinyProject.Application.DTO.Album;
using CoinyProject.Application.DTO.Discussion;
using CoinyProject.Core.Domain.Entities;
using CoinyProject.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Application.AlbumServices.Services
{
    public class DiscussionService :IDiscussionService
    {
        private readonly ApplicationDBContext _dBContext;
        private readonly IMapper _mapper;
        public DiscussionService(ApplicationDBContext dBContext, IMapper mapper)
        {
            _dBContext = dBContext;
            _mapper = mapper;
        }

        public async Task AddDiscussion(DiscussionCreateDTO discussion, string userId)
        {
            var _discussion = _mapper.Map<Discussion>(discussion);
            _discussion.UserId = userId;

            await _dBContext.Discussions.AddAsync(_discussion);
            await _dBContext.SaveChangesAsync();

        }

        public async Task<IEnumerable<DiscussionTopicDTO>> GetAvailableTopics()
        {
            var topics = await _dBContext.DiscussionTopics
                .AsNoTracking()
                .ToListAsync();

            var _topics = new List<DiscussionTopicDTO>();
            foreach (var topic in topics)
            {
                _topics.Add(_mapper.Map<DiscussionTopicDTO>(topic));
            }
            return _topics;
        }

        public async Task<IEnumerable<DiscussionGetForViewDTO>> GetAllDiscussionsForView()
        {
            var _discussions = await _dBContext.Discussions
                .Include(x => x.User)
                .Include(x => x.DiscussionTopic)
                .AsNoTracking()
                .ToListAsync();
            
                var discussions = new List<DiscussionGetForViewDTO>();

                foreach (var discussion in _discussions)
                {
                    discussions.Add(new DiscussionGetForViewDTO()
                    {
                        Id = discussion.Id,
                        Name = discussion.Name,
                        Username = discussion.User.UserName,
                        Topic = discussion.DiscussionTopic.Name
                    });
                }
                return discussions;
        }
    }
}
