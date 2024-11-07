using AutoMapper;
using CoinyProject.Application.Abstractions.DataServices;
using CoinyProject.Application.Dto.AlbumElement;
using CoinyProject.Domain.Entities;

namespace CoinyProject.Application.AutoMapper.Resolvers;

public class GetImageUrlResolver : IValueResolver<AlbumElement, AlbumElementGetDto, string>, IValueResolver<AlbumElement, AlbumElementViewGetDto, string>
{
    private readonly IFileService _fileService;

    public GetImageUrlResolver(IFileService fileService)
    {
        _fileService = fileService;
    }


    public string Resolve(AlbumElement source, AlbumElementGetDto destination, string destMember,
        ResolutionContext context)
    {
        return _fileService.GetImageUrl(source.ImageUrl);
    }

    public string Resolve(AlbumElement source, AlbumElementViewGetDto destination, string destMember, ResolutionContext context)
    {
        return _fileService.GetImageUrl(source.ImageUrl);
    }
}
