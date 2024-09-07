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

        public async Task<PagedResponse<Album>> GetPagedActiveAlbumsWithElementsAsync(int page, int size)
        {
            var query = Context.Albums
                .Include(x => x.Elements)
                .OrderByDescending(x => x.Rate)
                .Where(x => x.Status == AlbumStatus.Active)
                .AsNoTracking();

            return await GetPagedEntitiesAsync(query, page, size);
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