using CoinyProject.Application.Abstractions.Repositories;
using CoinyProject.Application.Dto.Other;
using CoinyProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoinyProject.Infrastructure.Repositories
{
    public class AlbumElementRepository : GenericRepository<AlbumElement>, IAlbumElementRepository
    {
        public AlbumElementRepository(ApplicationDbContext context) : base(context) {}

        public Task<PagedResponse<AlbumElement>> GetPagedAlbumElementsByAlbumIdAsync(Guid id, int page, int size)
        {
            var query = Context.AlbumElements
                .Where(x => x.AlbumId == id)
                .OrderByDescending(x => x.Rate)
                .AsNoTracking();

            return GetPagedEntitiesAsync(query, page, size);
        }
    }
}
