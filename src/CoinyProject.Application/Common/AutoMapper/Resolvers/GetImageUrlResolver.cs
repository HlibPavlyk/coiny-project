using AutoMapper;
using CoinyProject.Application.Abstractions.Data;
using CoinyProject.Application.Common.Models;
using CoinyProject.Domain.Entities;

namespace CoinyProject.Application.Common.AutoMapper.Resolvers;

public class GetImageUrlResolver(IFileService fileService) : IValueResolver<AlbumElement, AlbumElementLink, Uri>
{
    public Uri Resolve(AlbumElement source, AlbumElementLink destination, Uri destMember, ResolutionContext context)
    {
        return fileService.GetImageUrl(source.ImageUrl);
    }
}
