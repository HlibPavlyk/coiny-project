using CoinyProject.Application.DTO;
using CoinyProject.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Application.AlbumServices.Interfaces
{
    public interface IAlbumService
    {
        Task<int> AddAlbum(AlbumCreating album);
        Task AddAlbumElement(AlbumElementCreating albumElement);
        Task<IEnumerable<AlbumGetDTO>> GetAllAlbumsDTO();
        Task<AlbumGetByIdDTO> GetAlbumById(int id);
        Task<AlbumEditDTO> GetAlbumForEdit(int id);
        Task UpdateAlbum(AlbumEditDTO album);
        Task DeleteAlbum(int id);
    }
}
