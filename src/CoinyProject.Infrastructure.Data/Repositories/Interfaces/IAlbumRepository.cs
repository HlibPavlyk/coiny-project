using CoinyProject.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Infrastructure.Data.Repositories.Interfaces
{
    public interface IAlbumRepository : IBaseRepository<Album>
    {
        Task<IEnumerable<Album>?> GetAllAlbumsWithElements(string? userId);
        Task<IEnumerable<Album>?> GetAllAlbumsWithElementsAndFavoritesForView();
        Task<Album?> GetAlbumWithElementsById(int? id);
        Task<Album?> GetAlbumWithAuthorCheck(int? id, string? currentUserId);
        Task<Album?> GetAlbumById(int? id);
    }
}
