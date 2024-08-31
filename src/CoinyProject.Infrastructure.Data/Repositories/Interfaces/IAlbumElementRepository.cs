using CoinyProject.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Infrastructure.Data.Repositories.Interfaces
{
    public interface IAlbumElementRepository : IBaseRepository<AlbumElement>
    {
        Task<AlbumElement?> GetAlbumElementWithAuthorCheck(int? id, string? currentUserId);
        Task<AlbumElement?> GetAlbumElementById(int? id);
    }
}
