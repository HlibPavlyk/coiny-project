using CoinyProject.Application.Dto.Album;
using CoinyProject.Application.DTO.Album;
using CoinyProject.Application.Dto.AlbumElement;
using CoinyProject.Application.Dto.Other;

namespace CoinyProject.Application.Abstractions.Services;

public interface IAlbumElementService
{
    Task<Guid> AddAlbumElement(AlbumElementPostDto element);
    Task<PagedResponse<AlbumElementGetDto> > GetPagedAlbumElementsByAlbumIdAsync(Guid id, int page, int size);
    Task<AlbumElementGetDto> GetAlbumElementByIdAsync(Guid id);
    Task<Guid> UpdateAlbumElementAsync(Guid id, AlbumElementPatchDto element);
    Task DeleteAlbumElementAsync(Guid id);
    
}