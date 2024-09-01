using AutoMapper;
using CoinyProject.Application.AlbumServices.Interfaces;
using CoinyProject.Application.DTO.Album;
using CoinyProject.Application.DTO.Discussion;
using CoinyProject.Core.Domain.Entities;
using CoinyProject.Infrastructure;
using CoinyProject.Infrastructure.Migrations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CoinyProject.Application.Abstractions.Repositories;
using CoinyProject.Application.Abstractions.Services;
using CoinyProject.Application.Dto.Album;
using static Azure.Core.HttpHeader;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CoinyProject.Application.AlbumServices.Services
{
    public class AlbumService : IAlbumService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMapper _mapper;

        private readonly IUnitOfWork _unitOfWork;

        private readonly string imageFolder = "albums/elements/";

        public AlbumService(IWebHostEnvironment webHostEnvironment, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _webHostEnvironment = webHostEnvironment;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }
        protected async Task<string> ConvertToImageUrl(IFormFile image)
        {
            string folder = imageFolder + Guid.NewGuid().ToString() + "_" + image.FileName;

            await image.CopyToAsync(new FileStream(Path.Combine(_webHostEnvironment.WebRootPath, folder), FileMode.Create));

            return "/" + folder;
        }

        public async Task<int> AddAlbum(AlbumCreating? album, string? userId)
        {
            if (album == null || userId.IsNullOrEmpty())
                throw new ArgumentNullException("Album or userId is null");

            var _album = _mapper.Map<Album>(album);
            _album.UserId = userId;

            await _unitOfWork.Albums.InsertAsync(_album);
            await _unitOfWork.SaveChangesAsync();

            return _album.Id;
        }


        public async Task AddAlbumElement(AlbumElementCreating? element)
        {
            if(element == null)
                throw new ArgumentNullException("Element is null");

            var album = await _unitOfWork.Albums.GetAlbumWithElementsById(element.AlbumId);
                
            if (album != null)
            {
                AlbumElement _albumElement = new AlbumElement()
                {
                    Name = element.Name,
                    Description = element.Description,
                    ImageURL = await ConvertToImageUrl(element.Image),
                };

                album.Elements.Add(_albumElement);
                await _unitOfWork.SaveChangesAsync();
            }
           
        }

        public async Task<IEnumerable<AlbumGetDTO>> GetAllAlbumsDTO(string? userId)
        {
            if (userId == null)
                throw new ArgumentNullException("userId is null");

            var albums = await _unitOfWork.Albums.GetAllAlbumsWithElements(userId);

            if (albums == null)
                throw new ArgumentNullException("albums is null");

            return _mapper.Map<List<AlbumGetDTO>>(albums);
        }

        public async Task<IEnumerable<AlbumGetForViewDTO>> GetAllAlbumsForView(string? userId)
        {
            var albums = await _unitOfWork.Albums.GetAllAlbumsWithElementsAndFavoritesForView();

            if (albums == null)
                throw new ArgumentNullException("albums is null");

            var albumsGetDTOList = new List<AlbumGetForViewDTO>();
            foreach(var album in albums)
            {
                var albumGetDTO = _mapper.Map<AlbumGetForViewDTO>(album);
                albumGetDTO.IsFavorite = album.FavoriteAlbums.Any(x => x.UserId == userId);
                albumsGetDTOList.Add(albumGetDTO);
            }
            
            return albumsGetDTOList;

        }

        public async Task<AlbumGetByIdDTO> GetAlbumById(int? id)
        {
            if(id == null)
                throw new ArgumentNullException("id is null");

            var album = await _unitOfWork.Albums.GetAlbumWithElementsById(id);

            if (album == null)
                throw new ArgumentNullException("album is null");

            return _mapper.Map<AlbumGetByIdDTO>(album);
        }

        public async Task<AlbumEditDTO> GetAlbumForEdit(int? id, string? currentUserId)
        {
            if (id == null || currentUserId == null)
                throw new ArgumentNullException("id or currentUserId is null");

            var album = await _unitOfWork.Albums.GetAlbumWithAuthorCheck(id, currentUserId);

            if (album == null)
                throw new ArgumentNullException("album is null");

            return _mapper.Map<AlbumEditDTO>(album);
        }
        public async Task UpdateAlbum(AlbumEditDTO? album)
        {
            if (album == null)
                throw new ArgumentNullException("album is null");

            var _album = await _unitOfWork.Albums.GetAlbumById(album.Id);

            if (_album != null)
            {
                _mapper.Map(album, _album);
                _unitOfWork.Albums.Update(_album);
                _unitOfWork.SaveChanges();
            }
        }

        public async Task DeleteAlbum(int? id, string? currentUserId)
        {
            if (id == null || currentUserId == null)
                throw new ArgumentNullException("id or currentUserId is null");

            var album = await _unitOfWork.Albums.GetAlbumWithAuthorCheck(id, currentUserId);

            if (album == null)
                throw new ArgumentNullException("album is null");

            _unitOfWork.Albums.Delete(album);
            _unitOfWork.SaveChanges();
        }

        public async Task<AlbumElementEditDto> GetAlbumElementForEdit(int? id, string? currentUserId)
        {
            if (id == null || currentUserId == null)
                throw new ArgumentNullException("id or currentUserId is null");

            var albumElement = await _unitOfWork.AlbumElements.GetAlbumElementWithAuthorCheck(id, currentUserId);

            if(albumElement == null)
                throw new ArgumentNullException("albumElement is null");

            return _mapper.Map<AlbumElementEditDto>(albumElement);

        }

        public async Task<int> UpdateAlbumElement(AlbumElementEditDto? element)
        {
            if (element == null)
                throw new ArgumentNullException("element is null");

            var _element = await _unitOfWork.AlbumElements.GetAlbumElementById(element.Id);

             if (_element == null)
                throw new ArgumentNullException("element is null");

            _element.Name = element.Name;
            _element.Description = element.Description;

            if(element.Image != null)
                _element.ImageURL = await ConvertToImageUrl(element.Image);

            _unitOfWork.AlbumElements.Update(_element);
            _unitOfWork.SaveChanges();

            return _element.AlbumId;
        }

        public async Task<int> DeleteAlbumElement(int? id, string? currentUserId)
        {
            if (id == null || currentUserId == null)
                throw new ArgumentNullException("id or currentUserId is null");

            var element = await _unitOfWork.AlbumElements.GetAlbumElementById(id);

            if (element == null)
                throw new ArgumentNullException("element is null");

            _unitOfWork.AlbumElements.Delete(element);
            _unitOfWork.SaveChanges();
            
            return element.AlbumId;
        }

        public async Task LikeAlbum(int albumId, string? currentUserId)
        {
            if (albumId == 0 || currentUserId == null)
                throw new ArgumentNullException("albumId or currentUserId is null");

            var user = await _unitOfWork.Users.GetUserWithFavoriteAlbumsById(currentUserId); 
            var album = await _unitOfWork.Albums.GetAlbumById(albumId);

            if (user != null && album != null)
            {
                if (!user.FavoriteAlbums.Any(a => a.AlbumId == albumId))
                {
                    user.FavoriteAlbums.Add(new FavoriteAlbums { UserId = currentUserId, AlbumId = albumId });
                    album.Rate++;
                }
                else
                {
                    _unitOfWork.FavoriteAlbums.Delete(user.FavoriteAlbums.FirstOrDefault(a => a.AlbumId == albumId));
                    album.Rate--;
                }

                await _unitOfWork.SaveChangesAsync();

            }

        }
    }
}
