﻿using AutoMapper;
using CoinyProject.Application.AutoMapper.Resolvers;
using CoinyProject.Application.Dto.Album;
using CoinyProject.Application.DTO.Album;
using CoinyProject.Application.Dto.AlbumElement;
using CoinyProject.Application.Dto.Other;
using CoinyProject.Application.Dto.User;
using CoinyProject.Domain.Entities;
using CoinyProject.Domain.Enums;

namespace CoinyProject.Application.AutoMapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap(typeof(PagedResponse<>), typeof(PagedResponse<>));
            
            CreateMap<AlbumPostDto, Album>();
            CreateMap<AlbumElementPostDto, AlbumElement>()
                .ForMember(dest => dest.ImageUrl, src => src.Ignore());

            CreateMap<Album, AlbumViewGetDto>()
                .ForMember(dest => dest.ImagesUrls, opt => opt.MapFrom<GetISeveralImageUrlsResolver>())
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.User));
            CreateMap<User, UserNameGetDto>();
            CreateMap<Album, AlbumGetDto>()
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.User));
            CreateMap<Album, AlbumMinDetailsGetDTO>()
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.User));
            
            CreateMap<AlbumElement, AlbumElementGetDto>()
                .ForMember(dest => dest.ImageUrl, src => src.MapFrom<GetImageUrlResolver>());
            CreateMap<AlbumElement, AlbumElementViewGetDto>()
                .ForMember(dest => dest.ImageUrl, src => src.MapFrom<GetImageUrlResolver>());
            
            CreateMap<AlbumPatchDto, Album>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<AlbumElementPatchDto, AlbumElement>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<User, UserStatsGetDto>()
                .ForMember(dest => dest.LikesCount, opt => opt.MapFrom(src => src.Albums.Where(x => x.Status == AlbumStatus.Active)
                    .Select(x => x.Rate).Sum()))
                .ForMember(dest => dest.AlbumsCount, opt => opt.MapFrom(src => src.Albums
                    .Where(x => x.Status == AlbumStatus.Active).ToList().Count))
                .ForMember(dest => dest.AlbumsElementsCount,
                    opt => opt.MapFrom(src => src.Albums.Where(x => x.Status == AlbumStatus.Active).ToList()
                        .SelectMany(x => x.Elements).Count()));




            /*CreateMap<Album, AlbumGetForViewDTO>()
                .ForMember(x => x.TitleImageURL,  opt =>
                    opt.MapFrom(src => src.Elements.FirstOrDefault().ImageURL));

            CreateMap<Album, AlbumViewGetDto>()
                .ForCtorParam(nameof(AlbumGetForViewDTO.TitleImageURL), opt =>
                    opt.MapFrom(src => src.Elements.FirstOrDefault().ImageURL));*/


            /*
            CreateMap<AlbumElement, AlbumElementGetDto>()
                .ForMember(dest => dest.ImageUrl, opt =>
                    opt.MapFrom<AlbumElement>((src, dest, destMember, context) =>
                        context.Options.ServiceProvider.GetService<IFileService>().TransformName(src.Name)));*/

            // CreateMap(typeof(PagedResponse<>), typeof(PagedResponse<>));
            /*CreateMap<Album, AlbumEditDTO>();
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
                .ForCtorParam(nameof(DiscussionMessageGetForViewDTO.Username), opt => opt.MapFrom(src => src.User.UserName));*/
        }
    }
}
