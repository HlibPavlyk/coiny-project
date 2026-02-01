using AutoMapper;
using CoinyProject.Application.Common.AutoMapper.Resolvers;
using CoinyProject.Application.Common.Models;
using CoinyProject.Domain.Entities;

namespace CoinyProject.Application.Common.AutoMapper;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Album, BaseLink>()
            .ForMember(dest => dest.Title, src => src.MapFrom(opt => opt.Name));

        CreateMap<User, BaseLink>()
            .ForMember(dest => dest.Title, src => src.MapFrom(opt => opt.UserName));

        CreateMap<AlbumElement, AlbumElementLink>()
            .ForMember(dest => dest.Title, src => src.MapFrom(opt => opt.Name))
            .ForMember(dest => dest.ImageUrl, src => src.MapFrom<GetImageUrlResolver>());
    }
}
