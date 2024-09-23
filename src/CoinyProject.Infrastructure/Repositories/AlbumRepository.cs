using CoinyProject.Application.Abstractions.Repositories;
using CoinyProject.Application.Dto.Other;
using CoinyProject.Domain.Entities;
using CoinyProject.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CoinyProject.Infrastructure.Repositories
{
    public class AlbumRepository : GenericRepository<Album>, IAlbumRepository
    {
        public AlbumRepository(ApplicationDbContext context) : base(context) {}

        /*public async Task<Album?> GetAlbumWithElementsByIdAsync(Guid id)
        {
            return await Context.Albums
                .Include(x => x.Elements)
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == id)
        }*/

        public async Task<PagedResponse<Album>> GetPagedActiveAlbumsWithElementsAsync(PageQueryDto pageQuery, SortByItemQueryDto? sortQuery)
        {
            var query = Context.Albums
                .Include(x => x.Elements)
                .Where(x => x.Status == AlbumStatus.Active)
                .AsNoTracking();

            if (sortQuery != null)
            {
                query = sortQuery.SortItem switch
                {
                    "rate" => sortQuery.IsAscending
                        ? query.OrderBy(x => x.Rate)
                        : query.OrderByDescending(x => x.Rate),
                    "time" => sortQuery.IsAscending
                        ? query.OrderBy(x => x.UpdatedAt)
                        : query.OrderByDescending(x => x.UpdatedAt),
                    _ => query
                };
            }

            return await GetPagedEntitiesAsync(query, pageQuery.Page, pageQuery.PageSize);
        }

        public async Task<PagedResponse<Album>> GetPagedAlbumsWithElementsAndFavoritesForViewAsync(int page, int size)
        {
            var query = Context.Albums
                .Include(x => x.Elements)
                .ThenInclude(e => e.FavoriteAlbumElements)
                .Where(x => x.Status == AlbumStatus.Active)
                .OrderByDescending(x => x.Rate)
                .AsNoTracking();

            return await GetPagedEntitiesAsync(query, page, size);
        }
    }
}