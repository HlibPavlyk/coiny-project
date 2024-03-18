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
        Task AddAlbum(AlbumCreating album);
        Task AddAlbumElement(AlbumElementCreating albumElement);
        Task<(string, string)> CommitAlbumCreation();
        Task<IEnumerable<AlbumGetDTO>> GetAllAlbumsDTO();
        Task<AlbumGetByIdDTO> GetAlbumById(int id);
    }
}
