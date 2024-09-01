using CoinyProject.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoinyProject.Application.Abstractions.Repositories;

namespace CoinyProject.Infrastructure.Repositories.Realization
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dBContext;
        public IAlbumRepository Albums { get; private set; }
        public IAlbumElementRepository AlbumElements { get; private set; }
        public IUserRepository Users { get; private set; }
        public IFavoriteAlbumRepository FavoriteAlbums { get; private set; }
        public IDiscussionRepository Discussions { get; private set; }
        public IDiscussionMessageRepository DiscussionMessages { get; private set; }
        public IDiscussionTopicRepository DiscussionTopics { get; private set; }

        public UnitOfWork(ApplicationDbContext dBContext)
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

        public Task SaveChangesAsync()
        {
            return _dBContext.SaveChangesAsync();
        }
    }
}
