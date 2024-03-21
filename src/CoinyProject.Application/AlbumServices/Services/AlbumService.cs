﻿using CoinyProject.Application.AlbumServices.Interfaces;
using CoinyProject.Application.DTO;
using CoinyProject.Core.Domain.Entities;
using CoinyProject.Infrastructure.Data;
using CoinyProject.Infrastructure.Data.Migrations;
using CoinyProject.Infrastructure.Data.Repositories;
using Microsoft.AspNetCore.Hosting;
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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AlbumService(ApplicationDBContext dBContext, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = new UnitOfWork(dBContext);
            _webHostEnvironment = webHostEnvironment;   
        }

        public async Task<int> AddAlbum(AlbumCreating album)
        {
            Album _album = new Album();

            _album.Name = album.Name;
            if (album.Description != null)
                _album.Description = album.Description;

            await _unitOfWork.AlbumRepository.Add(_album);
            _unitOfWork.Commit();

            return _album.Id;
        }
        public async Task AddAlbumElement(AlbumElementCreating element)
        {
            string imageURL = "";
            var album = _unitOfWork.AlbumRepository.Include(x => x.Elements)
                .Where(x => x.Id == element.AlbumId)
                .FirstOrDefault();

            if (element.Image != null)
            {
                string folder = "albums/elements/";
                folder += Guid.NewGuid().ToString() + "_" + element.Image.FileName;

                imageURL = "/" + folder;

                string serverFolder = Path.Combine(_webHostEnvironment.WebRootPath, folder);

                await element.Image.CopyToAsync(new FileStream(serverFolder, FileMode.Create));
            }   

            if (album != null)
            {
                AlbumElement _albumElement = new AlbumElement()
                {
                    Name = element.Name,
                    Description = element.Description,
                    ImageURL = imageURL
                };

                album.Elements.Add(_albumElement);
                _unitOfWork.Commit();
            }
           
        }

        public async Task<IEnumerable<AlbumGetDTO>> GetAllAlbumsDTO()
        {
            var alums = await _unitOfWork.AlbumRepository
                .Include(x => x.Elements)
                .AsNoTracking()
                .ToListAsync();

            var albumsGetDTOList = new List<AlbumGetDTO>();

            foreach(Album album in alums)
            {
                albumsGetDTOList.Add(new AlbumGetDTO()
                {
                    Id = album.Id,
                    Name = album.Name,
                    Description = album.Description,
                    Rate = album.Rate,
                    TitleImageURL = album.Elements?.FirstOrDefault()?.ImageURL
                });
            }
            return albumsGetDTOList;
        }

        public async Task<AlbumGetByIdDTO> GetAlbumById(int id)
        {
            var album = await _unitOfWork.AlbumRepository
                .Where(x => x.Id == id)
                .Include(x => x.Elements)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            var AlbumGetByIdDTO = new AlbumGetByIdDTO()
            {
                Id = album.Id,
                Name = album.Name,
                Rate = album.Rate,
                Description = album.Description,
                Elements = album.Elements.Select(x => new AlbumElementGetDTO()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    ImageURL = x.ImageURL
                }).ToList()
            };

            return AlbumGetByIdDTO;
        }

        public async Task<AlbumEditDTO> GetAlbumForEdit(int id)
        {
            var album = await _unitOfWork.AlbumRepository
                .Where(x => x.Id == id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            var albumEditDTO = new AlbumEditDTO()
            {
                Id = album.Id,
                Name = album.Name,
                Description = album.Description
            };

            return albumEditDTO;
        }
        public async Task UpdateAlbum(AlbumEditDTO album)
        {
            var _album = await _unitOfWork.AlbumRepository
                .Where(x => x.Id == album.Id)
                .FirstOrDefaultAsync();

            _album.Name = album.Name;
            _album.Description = album.Description;

            await _unitOfWork.AlbumRepository.Update(_album);
            _unitOfWork.Commit();

        }

        public async Task DeleteAlbum(int id)
        {
            var album = await _unitOfWork.AlbumRepository
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            await _unitOfWork.AlbumRepository.Remove(album);
            _unitOfWork.Commit();
        }
    }
}
