using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Core.Domain.Interfaces
{
    public interface IBaseRepository<TEntity> where TEntity : class
    {
        Task<TEntity> GetById(int id);
        Task<IEnumerable<TEntity>> GetAll();
        Task Add(TEntity entity);
        Task Remove(TEntity entity);
        Task Update(TEntity entity);
        IQueryable<TEntity> Include(Expression<Func<TEntity, object>> navigationProperty);

    }
}
