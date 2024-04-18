using CoinyProject.Infrastructure.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Infrastructure.Data.Repositories.Realization
{
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
    {
        private readonly ApplicationDBContext _context;
        public BaseRepository(ApplicationDBContext context)
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
