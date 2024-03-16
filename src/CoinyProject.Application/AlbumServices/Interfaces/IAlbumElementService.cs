﻿using CoinyProject.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Application.AlbumServices.Interfaces
{
    public interface IAlbumElementService
    {
        Task SetAlbumId(int albumId);
        Task AddAlbumElement(AlbumElementCreating albumElement);
        Task CommitAlbumElementList();


    }
}
