using AutoMapper;
using CoinyProject.Application.Abstractions.DataServices;
using CoinyProject.Application.Features.Albums.Models;
using CoinyProject.Domain.Entities;

namespace CoinyProject.Application.Common.AutoMapper.Resolvers;

public class GetISeveralImageUrlsResolver : IValueResolver<Album, AlbumViewGetModel, IEnumerable<string>>
{
    private readonly IFileService _fileService;

    public GetISeveralImageUrlsResolver(IFileService fileService)
    {
        _fileService = fileService;
    }

    public IEnumerable<string> Resolve(Album source, AlbumViewGetModel destination, IEnumerable<string> destMember, ResolutionContext context)
    {
        return source.Elements.Select(e => _fileService.GetImageUrl(e.ImageUrl))
            .Take(4).ToList();
    }
}