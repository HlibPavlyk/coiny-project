using CoinyProject.Application.Dto.Other;
using CoinyProject.Domain.Entities;

namespace CoinyProject.Application.Abstractions.Repositories
{
    public interface IAlbumElementRepository : IGenericRepository<AlbumElement>
    {
        Task<PagedResponse<AlbumElement>> GetPagedAlbumElementsByAlbumIdAsync(Guid id, int page, int size);
    }
}
