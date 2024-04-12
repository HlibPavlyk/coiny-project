using AutoMapper;
using CoinyProject.Application.AlbumServices.Interfaces;
using CoinyProject.Application.DTO.Album;
using CoinyProject.Application.DTO.Discussion;
using CoinyProject.Core.Domain.Entities;
using CoinyProject.Infrastructure.Data;
using CoinyProject.Infrastructure.Data.Interfaces;
using CoinyProject.Infrastructure.Data.Migrations;
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
using static Azure.Core.HttpHeader;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CoinyProject.Application.AlbumServices.Services
{
    public class AlbumService : IAlbumService
    {
        private readonly IApplicationDBContext _dBContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMapper _mapper;

        private readonly string imageFolder = "albums/elements/";

        public AlbumService(IApplicationDBContext dBContext, IWebHostEnvironment webHostEnvironment, IMapper mapper)
        {
            _webHostEnvironment = webHostEnvironment;
            _dBContext = dBContext;
            _mapper = mapper;
        }
        protected async Task<string> ConvertToImageUrl(IFormFile image)
        {
            string folder = imageFolder + Guid.NewGuid().ToString() + "_" + image.FileName;

            await image.CopyToAsync(new FileStream(Path.Combine(_webHostEnvironment.WebRootPath, folder), FileMode.Create));

            return "/" + folder;
        }

        protected async Task AlbumAuthorCheck(int albumId, string currentUserId)
        {
            var userId = await _dBContext.Albums
                .Where(x => x.Id == albumId)
                .AsNoTracking()
                .Select(x => x.UserId)
                .FirstOrDefaultAsync();

            if (userId != currentUserId)
                throw new UnauthorizedAccessException("Access is denied. Current user is not the author of album");
        }

        protected async Task AlbumAuthorCheckForElements(int albumElementId, string currentUserId)
        {
            var albumId = await _dBContext.AlbumElements
                .Where(x => x.Id == albumElementId)
                .AsNoTracking()
                .Select(x => x.AlbumId)
                .FirstOrDefaultAsync();

            await AlbumAuthorCheck(albumId, currentUserId);

        }

        public async Task<int> AddAlbum(AlbumCreating? album, string? userId)
        {
            if (album == null || userId.IsNullOrEmpty())
                throw new ArgumentNullException("Album or userId is null");

            var _album = _mapper.Map<Album>(album);
            _album.UserId = userId;

            await _dBContext.Albums.AddAsync(_album);
            await _dBContext.SaveChangesAsync();

            return _album.Id;
        }


        public async Task AddAlbumElement(AlbumElementCreating element)
        {
            var album = await _dBContext.Albums
                .Include(x => x.Elements)
                .Where(x => x.Id == element.AlbumId)
                .FirstOrDefaultAsync();

            if (album != null)
            {
                AlbumElement _albumElement = new AlbumElement()
                {
                    Name = element.Name,
                    Description = element.Description,
                    ImageURL = await ConvertToImageUrl(element.Image),
                };

                album.Elements.Add(_albumElement);
                await _dBContext.SaveChangesAsync();
            }
           
        }

        public async Task<IEnumerable<AlbumGetDTO>> GetAllAlbumsDTO(string userId)
        {
            var albums = await _dBContext.Albums
                                       .Include(x => x.Elements)
                                       .AsNoTracking()
                                       .Where(x => x.UserId == userId)
                                       .ToListAsync();

            return _mapper.Map<List<AlbumGetDTO>>(albums);
        }

        public async Task<IEnumerable<AlbumGetForViewDTO>> GetAllAlbumsForView(string userId)
        {
            var albums = await _dBContext.Albums
                                .Include(x => x.Elements)
                                .Include(x => x.FavoriteAlbums)
                                .Where(x => x.Elements.Count > 0)
                                .OrderByDescending(x => x.Rate)
                                .AsNoTracking()
                                .ToListAsync();

            var albumsGetDTOList = albums.Select(album =>
            {
                var albumGetDTO = _mapper.Map<AlbumGetForViewDTO>(album);
                albumGetDTO.IsFavorite = album.FavoriteAlbums.Any(a => a.UserId == userId);
                return albumGetDTO;
            }).ToList();

            return albumsGetDTOList;

        }

        public async Task<AlbumGetByIdDTO> GetAlbumById(int id)
        {
            var album = await _dBContext.Albums
                .Where(x => x.Id == id)
                .Include(x => x.Elements)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return _mapper.Map<AlbumGetByIdDTO>(album);
        }

        public async Task<AlbumEditDTO> GetAlbumForEdit(int id, string currentUserId)
        {
            await AlbumAuthorCheck(id, currentUserId);

            var album = await _dBContext.Albums
                .Where(x => x.Id == id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return _mapper.Map<AlbumEditDTO>(album);
        }
        public async Task UpdateAlbum(AlbumEditDTO album)
        {
            var _album = await _dBContext.Albums
                .Where(x => x.Id == album.Id)
                .FirstOrDefaultAsync();

            if (_album != null)
            {
                _mapper.Map(album, _album);
                _dBContext.Albums.Update(_album);
                _dBContext.SaveChanges();
            }
        }

        public async Task DeleteAlbum(int id, string currentUserId)
        {
            await AlbumAuthorCheck(id, currentUserId);

            var album = await _dBContext.Albums
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            _dBContext.Albums.Remove(album);
            _dBContext.SaveChanges();
        }

        public async Task<AlbumElementEditDTO> GetAlbumElementForEdit(int id, string currentUserId)
        {
            await AlbumAuthorCheckForElements(id, currentUserId);

            var albumElement = await _dBContext.AlbumElements
                .Where(x => x.Id == id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return _mapper.Map<AlbumElementEditDTO>(albumElement);

        }

        public async Task<int> UpdateAlbumElement(AlbumElementEditDTO element)
        {
            var _element = await _dBContext.AlbumElements
                .Where(x => x.Id == element.Id)
                .FirstOrDefaultAsync();

            _element.Name = element.Name;
            _element.Description = element.Description;

            if(element.Image != null)
                _element.ImageURL = await ConvertToImageUrl(element.Image);

            _dBContext.AlbumElements.Update(_element);
            _dBContext.SaveChanges();

            return _element.AlbumId;
        }

        public async Task<int> DeleteAlbumElement(int id, string currentUserId)
        {
            await AlbumAuthorCheckForElements(id, currentUserId);

            var element = await _dBContext.AlbumElements
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            _dBContext.AlbumElements.Remove(element);
            _dBContext.SaveChanges();
            
            return element.AlbumId;
        }

        public async Task LikeAlbum(int albumId, string currentUserId)
        {
            var user = await _dBContext.Users
                .Include(u => u.FavoriteAlbums)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);
            var album = await _dBContext.Albums.FirstOrDefaultAsync(a => a.Id == albumId);

            if (user != null && album != null)
            {
                if (!user.FavoriteAlbums.Any(a => a.AlbumId == albumId))
                {
                    user.FavoriteAlbums.Add(new FavoriteAlbums { UserId = currentUserId, AlbumId = albumId });
                    album.Rate++;
                }
                else
                {
                    _dBContext.FavoriteAlbums.Remove(user.FavoriteAlbums.FirstOrDefault(a => a.AlbumId == albumId));
                    album.Rate--;
                }

                await _dBContext.SaveChangesAsync();

            }

        }
    }
}
