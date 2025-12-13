using CoinyProject.Application.Dto.Album;
using CoinyProject.Application.Dto.Other;
using CoinyProject.Application.Features.Albums.Models;

namespace CoinyProject.Application.Abstractions.Services
{
    public interface IAlbumService
    {
        Task<Guid> AddAlbumAsync(AlbumPostDto album);
        Task<PagedResponse<AlbumViewGetModel>> GetPagedAlbumsAsync(PageQueryDto pageQuery, SortByItemQueryDto? sortQuery,
            string? search);
        Task<PagedResponse<AlbumViewGetModel>> GetPagedActiveAlbumsByUserIdAsync(Guid userId, PageQueryDto pageQuery,
            SortByItemQueryDto? sortQuery, string? search);
        Task<PagedResponse<AlbumViewGetModel>> GetCurrentUserPagedAlbumsAsync(PageQueryDto pageQuery,
            SortByItemQueryDto? sortQuery, string? search);
        Task<AlbumGetDto> GetAlbumById(Guid id);
        Task<Guid> UpdateAlbumAsync(Guid id, AlbumPatchDto album);
        Task DeactivateAlbumAsync(Guid id);
        Task ActivateAlbumAsync(Guid id);
        Task ApproveAlbumAsync(Guid id);
       
    }
}
