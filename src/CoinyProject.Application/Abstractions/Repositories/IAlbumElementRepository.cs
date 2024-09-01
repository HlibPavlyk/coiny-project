using CoinyProject.Domain.Entities;

namespace CoinyProject.Application.Abstractions.Repositories
{
    public interface IAlbumElementRepository : IBaseRepository<AlbumElement>
    {
        Task<AlbumElement?> GetAlbumElementWithAuthorCheck(int? id, string? currentUserId);
        Task<AlbumElement?> GetAlbumElementById(int? id);
    }
}
