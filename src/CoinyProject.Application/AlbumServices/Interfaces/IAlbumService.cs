using CoinyProject.Application.DTO;
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
    }
}
