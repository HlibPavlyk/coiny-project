using CoinyProject.Application.Abstractions.Repositories;

namespace CoinyProject.Infrastructure.Repositories
{
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
    {
        private readonly ApplicationDbContext _context;
        public BaseRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public void Delete(TEntity entity)
        {
            _context.Remove(entity);
        }

        public async Task InsertAsync(TEntity entity)
        {
            await _context.AddAsync(entity);
        }

        public void Update(TEntity entity)
        {
            _context.Update(entity);
        }
    }
}
