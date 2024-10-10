using CoinyProject.Application.Dto.Other;
using CoinyProject.Domain.Entities;

namespace CoinyProject.Application.Abstractions.Repositories
{
    public interface IAlbumElementRepository : IGenericRepository<AlbumElement>
    {
        Task<PagedResponse<AlbumElement>> GetPagedAlbumElementsByAlbumIdAsync(Guid id, PageQueryDto pageQuery,
            SortByItemQueryDto? sortQuery, string? search);
        
        Task<AlbumElement?> GetAlbumElementWithAlbumByIdAsync(Guid id);
    }
}
