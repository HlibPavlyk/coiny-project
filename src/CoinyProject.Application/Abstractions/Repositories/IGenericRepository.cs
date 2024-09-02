namespace CoinyProject.Application.Abstractions.Repositories
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(int id);
        Task<IEnumerable<TEntity>?> GetAllAsync();
        Task AddAsync(TEntity entity);
        void Remove(TEntity entity);
    }
}
