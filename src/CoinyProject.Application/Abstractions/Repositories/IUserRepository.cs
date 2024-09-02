using CoinyProject.Domain.Entities;

namespace CoinyProject.Application.Abstractions.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetUserWithFavoriteAlbumElementsById(Guid id);
    }
}
