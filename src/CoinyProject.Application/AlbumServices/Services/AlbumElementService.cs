using CoinyProject.Application.AlbumServices.Interfaces;
using CoinyProject.Application.DTO;
using CoinyProject.Core.Domain.Entities;
using CoinyProject.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Application.AlbumServices.Services
{
    public class AlbumElementService : IAlbumElementService
    {
        private readonly UnitOfWork _unitOfWork;
        private int albumId;
        public Task SetAlbumId(int id)
        {
            if(id != 0)
                albumId = id;

            return Task.CompletedTask;
        }
   

        public AlbumElementService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public async Task AddAlbumElement(AlbumElementCreating element)
        {
            if(albumId != 0)
            {
                AlbumElement _albumElement = new AlbumElement();

                _albumElement.AlbumId = albumId;
                _albumElement.Name = element.Name;

                if (_albumElement.Description != null)
                    _albumElement.Description = element.Description;

                await _unitOfWork.AlbumElementRepository.Add(_albumElement);
            }
        }

        public async Task CommitAlbumElementList()
        {
            if (albumId != 0)
            {
                await _unitOfWork.Commit();
            }
            albumId = 0;
        }
    }
}
