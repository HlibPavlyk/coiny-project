using System.Linq.Expressions;
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
        
        public async Task<PagedResponse<Album>> GetPagedActiveAlbumsWithElementsAsync(PageQueryDto pageQuery, SortByItemQueryDto? sortQuery)
        {
            return await GetPagedAlbumsWithElementsByPredicateAsync(pageQuery, sortQuery, x => x.Status == AlbumStatus.Active);
        }

        public Task<PagedResponse<Album>> GetPagedAlbumsWithElementsByUserIdAsync(Guid userId, PageQueryDto pageQuery, SortByItemQueryDto? sortQuery)
        {
            return GetPagedAlbumsWithElementsByPredicateAsync(pageQuery, sortQuery, x => x.UserId == userId);
        }

        public Task<PagedResponse<Album>> GetPagedActiveAlbumsWithElementsByUserIdAsync(Guid userId, PageQueryDto pageQuery, SortByItemQueryDto? sortQuery)
        {
            return GetPagedAlbumsWithElementsByPredicateAsync(pageQuery, sortQuery, x => x.UserId == userId && x.Status == AlbumStatus.Active);
        }

        public async Task<Album?> GetAlbumWithUserByIdAsync(Guid id)
        {
            return await Context.Albums
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        private async Task<PagedResponse<Album>> GetPagedAlbumsWithElementsByPredicateAsync(
            PageQueryDto pageQuery, 
            SortByItemQueryDto? sortQuery, 
            Expression<Func<Album, bool>>? predicate = null)
        {
            var query = Context.Albums
                .Include(x => x.Elements)
                .Include(x => x.User)
                .AsNoTracking();

            if (predicate != null)
                query = query.Where(predicate);

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
        
    }
}