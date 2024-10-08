using System.Linq.Expressions;
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
        
        private async Task<PagedResponse<Album>> GetPagedAlbumsWithElementsByPredicateAsync(
            PageQueryDto pageQuery, 
            SortByItemQueryDto? sortQuery,
            string? search,
            Expression<Func<Album, bool>>? predicate = null)
        {
            var query = Context.Albums
                .Include(x => x.Elements)
                .Include(x => x.User)
                .AsNoTracking();

            if (predicate != null)
                query = query.Where(predicate);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => x.Name.ToLower().Contains(search.ToLower()));
            }


            
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
                    "status" => sortQuery.IsAscending
                        ? query.OrderBy(x => x.Status)
                        : query.OrderByDescending(x => x.Status),
                    _ => throw new ArgumentException("Invalid sort item.")
                };
            }

            return await GetPagedEntitiesAsync(query, pageQuery.Page, pageQuery.PageSize);
        }

        public async Task<PagedResponse<AlbumElement>> GetPagedAlbumElementsByAlbumIdAsync(Guid id, PageQueryDto pageQuery, SortByItemQueryDto? sortQuery,
            string? search)
        {
            var query = Context.AlbumElements
                .Where(x => x.AlbumId == id)
                .AsNoTracking();
     
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => x.Name.ToLower().Contains(search.ToLower()));
            }
            
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
                    _ => throw new ArgumentException("Invalid sort item.")
                };
            }

            return await GetPagedEntitiesAsync(query, pageQuery.Page, pageQuery.PageSize);
        }
    }
}
