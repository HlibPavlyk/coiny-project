using CoinyProject.Domain.Entities;

namespace CoinyProject.Application.Abstractions.Repositories
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> GetUserWithFavoriteAlbumsById(string? id);
    }
}
