namespace CoinyProject.Application.Abstractions.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IAlbumRepository Albums { get; }
        IAlbumElementRepository AlbumElements { get; }
        IUserRepository Users { get; }
        IFavoriteAlbumRepository FavoriteAlbums { get; }
        IDiscussionRepository Discussions { get; }
        IDiscussionMessageRepository DiscussionMessages { get; }
        Task SaveChangesAsync();
    }
}
