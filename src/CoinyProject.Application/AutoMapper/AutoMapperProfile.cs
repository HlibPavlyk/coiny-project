using AutoMapper;
using CoinyProject.Application.Dto.Album;
using CoinyProject.Application.DTO.Album;
using CoinyProject.Application.DTO.Discussion;
using CoinyProject.Domain.Entities;

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
            CreateMap<AlbumElement, AlbumElementGetDto>();
            
            CreateMap<Album, AlbumEditDTO>();
            CreateMap<AlbumEditDTO, Album>();

            CreateMap<AlbumElement, AlbumElementEditDto>();
            CreateMap<AlbumElementEditDto, AlbumElement>();

            CreateMap<DiscussionCreateDTO, Discussion>(); 


            CreateMap<DiscussionMessageCreateDTO, DiscussionMessage>();

            CreateMap<Discussion, DiscussionGetForViewDTO>()
                .ForCtorParam(nameof(DiscussionGetForViewDTO.Username), opt => opt.MapFrom(src => src.User.UserName));

            CreateMap<Discussion, DiscussionGetByIdDTO>()
                .ForCtorParam(nameof(DiscussionGetByIdDTO.Username), opt => opt.MapFrom(src => src.User.UserName));

            CreateMap<DiscussionMessage, DiscussionMessageGetForViewDTO>()
                .ForCtorParam(nameof(DiscussionMessageGetForViewDTO.Username), opt => opt.MapFrom(src => src.User.UserName));
        }
    }
}
