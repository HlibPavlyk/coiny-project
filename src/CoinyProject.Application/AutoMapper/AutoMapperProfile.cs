using AutoMapper;
using CoinyProject.Application.DTO;
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

        }
    }
}
