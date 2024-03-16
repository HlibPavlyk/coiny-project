using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Infrastructure.Data.Repositories
{
    public class UnitOfWork : IDisposable
    {
        private readonly ApplicationDBContext _dbContext;
        public AlbumRepository AlbumRepository { get; private set; }
        public AlbumElementRepository AlbumElementRepository { get; private set; }  

        public UnitOfWork(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
            AlbumElementRepository = new AlbumElementRepository(dbContext); 
            AlbumRepository = new AlbumRepository(dbContext);
        }
        public Task Commit() => _dbContext.SaveChangesAsync();

        public void Dispose() => _dbContext.Dispose();
    }
}
