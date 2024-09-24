using AutoMapper;
using CoinyProject.Application.Abstractions.DataServices;
using CoinyProject.Application.DTO.Album;
using CoinyProject.Domain.Entities;

namespace CoinyProject.Application.AutoMapper.Resolvers;

public class GetISeveralImageUrlsResolver : IValueResolver<Album, AlbumViewGetDto, IEnumerable<string>>
{
    private readonly IFileService _fileService;

    public GetISeveralImageUrlsResolver(IFileService fileService)
    {
        _fileService = fileService;
    }

    public IEnumerable<string> Resolve(Album source, AlbumViewGetDto destination, IEnumerable<string> destMember, ResolutionContext context)
    {
        return source.Elements.Select(e => _fileService.GetImageUrl(e.ImageUrl))
            .Take(4).ToList();
    }
}