using CoinyProject.Application.Abstractions.Repositories;

namespace CoinyProject.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;
        private bool _disposed;

        public IAlbumRepository Albums { get; }
        public IAlbumElementRepository AlbumElements { get; }
        public IUserRepository Users { get; }
        public IFavoriteAlbumElementRepository FavoriteAlbumElements { get; }
        public IDiscussionRepository Discussions { get; }
        public IDiscussionMessageRepository DiscussionMessages { get; }

        public UnitOfWork(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            Albums = new AlbumRepository(_dbContext);
            AlbumElements = new AlbumElementRepository(_dbContext);
            Users = new UserRepository(_dbContext);
            FavoriteAlbumElements = new FavoriteAlbumElementRepository(_dbContext);
            Discussions = new DiscussionRepository(_dbContext);
            DiscussionMessages = new DiscussionMessageRepository(_dbContext);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _dbContext.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
