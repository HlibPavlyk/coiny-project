using CoinyProject.Application.AlbumServices.Interfaces;
using CoinyProject.Application.DTO;
using CoinyProject.Core.Domain.Entities;
using CoinyProject.Infrastructure.Data;
using CoinyProject.Infrastructure.Data.Migrations;
using CoinyProject.Infrastructure.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CoinyProject.Application.AlbumServices.Services
{
    public class AlbumService : IAlbumService
    {
        private readonly UnitOfWork _unitOfWork;

        public AlbumService(ApplicationDBContext dBContext)
        {
            _unitOfWork = new UnitOfWork(dBContext);
        }

        public async Task AddAlbum(AlbumCreating album)
        {
            Album _album = new Album();

            _album.Name = album.Name;
            if (album.Description != null)
                _album.Description = album.Description;

            await _unitOfWork.AlbumRepository.Add(_album);
            _unitOfWork.Commit();
        }
        public Task AddAlbumElement(AlbumElementCreating element)
        {
            var album = _unitOfWork.AlbumRepository.Include(x => x.Elements)
                .Where(x => x.Elements.Count == 0)
                .OrderByDescending(x => x.Id)
                .FirstOrDefault();

            if (album != null)
            {
                AlbumElement _albumElement = new AlbumElement()
                {
                    Name = album.Name,
                    Description = album.Description,
                };

                album.Elements.Add(_albumElement);
                _unitOfWork.Commit();
            }

            return Task.CompletedTask;
        }

        public Task<(string,string)> CommitAlbumCreation()
        {
            var album = _unitOfWork.AlbumRepository.Include(x => x.Elements)
                .Where(x => !x.Elements.Any())
                .AsNoTracking()
                .FirstOrDefault();

            if (album != null)
                return Task.FromResult(("error", "At least one element must be provided"));
            else
                return Task.FromResult(("success", "Album successfule created"));
        }
    }
}
