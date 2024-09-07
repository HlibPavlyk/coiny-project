using CoinyProject.Application.Dto.Album;
using CoinyProject.Application.DTO.Album;
using CoinyProject.Application.Dto.Other;

namespace CoinyProject.Application.Abstractions.Services
{
    public interface IAlbumService
    {
        Task<Guid> AddAlbumAsync(AlbumPostDto album);
        Task<PagedResponse<AlbumGetDto>> GetPagedAlbumsAsync(int page, int size);
        Task<AlbumGetDto> GetAlbumById(Guid id);
        Task<Guid> UpdateAlbumAsync(Guid id, AlbumPatchDto album);
        Task DeactivateAlbumAsync(Guid id);
        Task ActivateAlbumAsync(Guid id);
        Task ApproveAlbumAsync(Guid id);
       
    }
}
