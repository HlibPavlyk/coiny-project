using CoinyProject.Application.DTO;
using CoinyProject.Core.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Application.AlbumServices.Interfaces
{
    public interface IAlbumService
    {
        Task<int> AddAlbum(AlbumCreating album, string userId);
        Task AddAlbumElement(AlbumElementCreating albumElement);
        Task<IEnumerable<AlbumGetDTO>> GetAllAlbumsDTO(string userId);
        Task<IEnumerable<AlbumGetDTO>> GetAllAlbumsForView();
        Task<AlbumGetByIdDTO> GetAlbumById(int id);
        Task<AlbumEditDTO> GetAlbumForEdit(int id, string currentUserId);
        Task UpdateAlbum(AlbumEditDTO album);
        Task DeleteAlbum(int id, string currentUserId);
        Task<AlbumElementEditDTO> GetAlbumElementForEdit(int id, string currentUserId);
        Task<int> UpdateAlbumElement(AlbumElementEditDTO album);
        Task<int> DeleteAlbumElement(int id, string currentUserId);
    }
}
