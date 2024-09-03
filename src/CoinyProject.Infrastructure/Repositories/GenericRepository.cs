using CoinyProject.Application.Abstractions.Repositories;
using CoinyProject.Application.Dto.Other;
using Microsoft.EntityFrameworkCore;

namespace CoinyProject.Infrastructure.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        protected readonly ApplicationDbContext Context;

        protected GenericRepository(ApplicationDbContext context)
        {
            Context = context;
        }

        public async Task<TEntity?> GetByIdAsync(int id)
        {
            return await Context.Set<TEntity>()
                .FindAsync(id);
        }

        public async Task<IEnumerable<TEntity>?> GetAllAsync()
        {
            return await Context.Set<TEntity>()
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddAsync(TEntity entity)
        {
            await Context.Set<TEntity>()
                .AddAsync(entity);
        }

        public void Remove(TEntity entity)
        {
            Context.Set<TEntity>()
                .Remove(entity);
        }
        
        protected async Task<PagedResponse<T>> GetPagedEntitiesAsync<T>(IQueryable<T> query,
            int page, int size) where T : class
        {
            var totalItems = await query.CountAsync();
            if (totalItems == 0)
            {
                return new PagedResponse<T>
                {
                    Items = new List<T>(),
                    TotalPages = 0
                };
            }

            var items = await query
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalItems / (double)size);

            return new PagedResponse<T>
            {
                Items = items,
                TotalPages = totalPages
            };
        }

    }
}