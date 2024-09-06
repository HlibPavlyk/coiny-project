using System.Linq.Expressions;

namespace CoinyProject.Application.Abstractions.Repositories
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity?> GetByIdAsync(Guid id);
        Task<IEnumerable<TEntity>?> GetAllAsync();
        Task AddAsync(TEntity entity);
        void Remove(TEntity entity);
    }
}
