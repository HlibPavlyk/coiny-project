using CoinyProject.Application.Dto.Album;
using CoinyProject.Application.DTO.Album;

namespace CoinyProject.Application.Abstractions.Services
{
    public interface IAlbumService
    {
        Task<int> AddAlbum(AlbumCreating? album, string? userId);
        Task AddAlbumElement(AlbumElementCreating? albumElement);
        Task<IEnumerable<AlbumGetDTO>> GetAllAlbumsDTO(string? userId);
        Task<IEnumerable<AlbumGetForViewDTO>> GetAllAlbumsForView(string? userId);
        Task<AlbumGetByIdDTO> GetAlbumById(int? id);
        Task<AlbumEditDTO> GetAlbumForEdit(int? id, string? currentUserId);
        Task UpdateAlbum(AlbumEditDTO? album);
        Task DeleteAlbum(int? id, string? currentUserId);
        Task<AlbumElementEditDto> GetAlbumElementForEdit(int? id, string? currentUserId);
        Task<int> UpdateAlbumElement(AlbumElementEditDto? album);
        Task<int> DeleteAlbumElement(int? id, string? currentUserId);
        Task LikeAlbum(int id, string? currentUserId);
    }
}
