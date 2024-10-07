using CoinyProject.Application.Dto.Other;
using CoinyProject.Domain.Entities;

namespace CoinyProject.Application.Abstractions.Repositories
{
    public interface IAlbumRepository : IGenericRepository<Album>
    {
        Task<PagedResponse<Album>> GetPagedActiveAlbumsWithElementsAsync(PageQueryDto pageQuery, SortByItemQueryDto? sortQuery);
        Task<PagedResponse<Album>> GetPagedAlbumsWithElementsByUserIdAsync(Guid userId, PageQueryDto pageQuery, SortByItemQueryDto? sortQuery);
        Task<PagedResponse<Album>> GetPagedActiveAlbumsWithElementsByUserIdAsync(Guid userId, PageQueryDto pageQuery, SortByItemQueryDto? sortQuery);
        Task<Album?> GetAlbumWithUserByIdAsync(Guid id);
    }
}
