using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Infrastructure.Data.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IAlbumRepository Albums { get; }
        IAlbumElementRepository AlbumElements { get; }
        IUserRepository Users { get; }
        IFavoriteAlbumRepository FavoriteAlbums { get; }
        IDiscussionRepository Discussions { get; }
        IDiscussionMessageRepository DiscussionMessages { get; }
        IDiscussionTopicRepository DiscussionTopics { get; }
        void SaveChanges();
        Task SaveChangesAsync();
    }
}
