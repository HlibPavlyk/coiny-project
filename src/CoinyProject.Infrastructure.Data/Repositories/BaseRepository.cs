using CoinyProject.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Infrastructure.Data.Repositories
{
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
    {
        protected readonly ApplicationDBContext _dbContext;
        public BaseRepository(ApplicationDBContext dBContext)
        {
            _dbContext = dBContext;
        }
        public Task Add(TEntity entity)
        {
            _dbContext.Add(entity);
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<TEntity>> GetAll()
        {
            return await _dbContext.Set<TEntity>().ToListAsync();
        }

        public async Task<TEntity> GetById(int id)
        {
            return await _dbContext.Set<TEntity>().FindAsync(id);
        }

        public Task Remove(TEntity entity)
        {
            _dbContext.Remove(entity);
            return Task.CompletedTask;
        }

        public Task Update(TEntity entity)
        {
            _dbContext.Update(entity);
            return Task.CompletedTask;
        }
    }
}
