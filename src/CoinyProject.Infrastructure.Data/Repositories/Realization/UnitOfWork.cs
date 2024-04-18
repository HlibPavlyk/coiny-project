using CoinyProject.Core.Domain.Entities;
using CoinyProject.Infrastructure.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Infrastructure.Data.Repositories.Realization
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDBContext _dBContext;
        public IAlbumRepository Albums { get; private set; }
        public IAlbumElementRepository AlbumElements { get; private set; }
        public IUserRepository Users { get; private set; }
        public IFavoriteAlbumRepository FavoriteAlbums { get; private set; }
        public IDiscussionRepository Discussions { get; private set; }
        public IDiscussionMessageRepository DiscussionMessages { get; private set; }
        public IDiscussionTopicRepository DiscussionTopics { get; private set; }

        public UnitOfWork(ApplicationDBContext dBContext)
        {
            _dBContext = dBContext;
            Albums = new AlbumRepository(_dBContext);
            AlbumElements = new AlbumElementRepository(_dBContext);
            Users = new UserRepository(_dBContext);
            FavoriteAlbums = new FavoriteAlbumRepository(_dBContext);
            Discussions = new DiscussionRepository(_dBContext);
            DiscussionMessages = new DiscussionMessageRepository(_dBContext);
            DiscussionTopics = new DiscussionTopicRepository(_dBContext);
        }

        public void Dispose()
        {
            _dBContext.Dispose();
        }

        public void SaveChanges()
        {
            _dBContext.SaveChanges();
        }

        public Task SaveChangesAsync()
        {
            return _dBContext.SaveChangesAsync();
        }
    }
}
