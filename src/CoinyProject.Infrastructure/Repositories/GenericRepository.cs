using CoinyProject.Application.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CoinyProject.Infrastructure.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        protected readonly ApplicationDbContext _context;

        protected GenericRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TEntity?> GetByIdAsync(int id)
        {
            return await _context.Set<TEntity>()
                .FindAsync(id);
        }

        public async Task<IEnumerable<TEntity>?> GetAllAsync()
        {
            return await _context.Set<TEntity>()
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddAsync(TEntity entity)
        {
            await _context.Set<TEntity>()
                .AddAsync(entity);
        }

        public void Remove(TEntity entity)
        {
            _context.Set<TEntity>()
                .Remove(entity);
        }

    }
}