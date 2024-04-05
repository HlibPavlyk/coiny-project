using AutoMapper;
using CoinyProject.Application.DTO.Album;
using CoinyProject.Application.DTO.Discussion;
using CoinyProject.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Application.AutoMapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<AlbumCreating, Album>();

            CreateMap<Album, AlbumGetByIdDTO>();
            CreateMap<AlbumElement, AlbumElementGetDTO>();
            
            CreateMap<Album, AlbumEditDTO>();
            CreateMap<AlbumEditDTO, Album>();

            CreateMap<AlbumElement, AlbumElementEditDTO>();
            CreateMap<AlbumElementEditDTO, AlbumElement>();

            CreateMap<DiscussionCreateDTO, Discussion>(); 

            CreateMap<DiscussionTopic, DiscussionTopicDTO>();

            CreateMap<DiscussionMessageCreateDTO, DiscussionMessage>();

            CreateMap<Discussion, DiscussionGetForViewDTO>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.UserName))
            .ForMember(dest => dest.Topic, opt => opt.MapFrom(src => src.DiscussionTopic.Name));

            CreateMap<Discussion, DiscussionGetByIdDTO>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.UserName))
            .ForMember(dest => dest.Topic, opt => opt.MapFrom(src => src.DiscussionTopic.Name));

            CreateMap<DiscussionMessage, DiscussionMessageGetForViewDTO>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.UserName));
        }
    }
}
