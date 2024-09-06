using AutoMapper;
using CoinyProject.Application.Abstractions.DataServices;
using CoinyProject.Application.DTO.Album;
using CoinyProject.Domain.Entities;

namespace CoinyProject.Application.AutoMapper.Resolvers;

public class ImageUrlResolver : IValueResolver<AlbumElement, AlbumElementGetDto, string>
{
    private readonly IFileService _fileService;

    public ImageUrlResolver(IFileService fileService)
    {
        _fileService = fileService;
    }


    public string Resolve(AlbumElement source, AlbumElementGetDto destination, string destMember,
        ResolutionContext context)
    {
        return _fileService.GetImageUrl(source.ImageUrl);
    }
}
