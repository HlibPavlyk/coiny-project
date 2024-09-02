using CoinyProject.Domain.Entities;

namespace CoinyProject.Application.Abstractions.Repositories
{
    public interface IAlbumElementRepository : IGenericRepository<AlbumElement>
    {
        Task<AlbumElement?> GetAlbumElementWithAuthorCheck(Guid id, Guid currentUserId);
        Task<AlbumElement?> GetAlbumElementById(Guid id);
    }
}
