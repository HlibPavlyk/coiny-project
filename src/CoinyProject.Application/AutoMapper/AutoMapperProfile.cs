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

            CreateMap<Album, AlbumGetForViewDTO>()
                .ForMember(x => x.TitleImageURL,  opt =>
                    opt.MapFrom(src => src.Elements.FirstOrDefault().ImageURL));

            CreateMap<Album, AlbumGetDTO>()
                .ForCtorParam(nameof(AlbumGetForViewDTO.TitleImageURL), opt => 
                    opt.MapFrom(src => src.Elements.FirstOrDefault().ImageURL));
            
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
                .ForCtorParam(nameof(DiscussionGetForViewDTO.Topic), opt => opt.MapFrom(src => src.DiscussionTopic.Name))
                .ForCtorParam(nameof(DiscussionGetForViewDTO.Username), opt => opt.MapFrom(src => src.User.UserName));

            CreateMap<Discussion, DiscussionGetByIdDTO>()
                .ForCtorParam(nameof(DiscussionGetByIdDTO.Username), opt => opt.MapFrom(src => src.User.UserName))
                .ForCtorParam(nameof(DiscussionGetByIdDTO.Topic), opt => opt.MapFrom(src => src.DiscussionTopic.Name));

            CreateMap<DiscussionMessage, DiscussionMessageGetForViewDTO>()
                .ForCtorParam(nameof(DiscussionMessageGetForViewDTO.Username), opt => opt.MapFrom(src => src.User.UserName));
        }
    }
}
