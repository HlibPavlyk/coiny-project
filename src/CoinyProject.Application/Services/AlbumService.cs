using System.Security.Claims;
using AutoMapper;
using CoinyProject.Application.Abstractions.Repositories;
using CoinyProject.Application.Abstractions.Services;
using CoinyProject.Application.Dto.Album;
using CoinyProject.Application.DTO.Album;
using CoinyProject.Domain.Entities;
using CoinyProject.Domain.Exceptions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;


namespace CoinyProject.Application.Services
{
    public class AlbumService : IAlbumService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

       // private readonly string imageFolder = "albums/elements/";

       public AlbumService(IMapper mapper, IUnitOfWork unitOfWork, IHostingEnvironment hostingEnvironment,
           IHttpContextAccessor httpContextAccessor)
       {
           _mapper = mapper;
           _unitOfWork = unitOfWork;
           _hostingEnvironment = hostingEnvironment;
           _httpContextAccessor = httpContextAccessor;
       }
        
        /*protected async Task<int> SaveImageAsync(IFormFile photo, int userId)
        {
            using (var memoryStream = new MemoryStream())
            {
                await photo.CopyToAsync(memoryStream);

                var imageEntity = new ImageEntity
                {
                    FileName = photo.FileName,
                    ContentType = photo.ContentType,
                    Data = memoryStream.ToArray(),
                    UserId = userId
                };

                using (var context = new ApplicationDbContext(/* options here #1#))
                {
                    context.Images.Add(imageEntity);
                    await context.SaveChangesAsync();
                }

                return imageEntity.Id;
            }
        }*/
        
        /*protected async Task<string> ConvertToImageUrl(IFormFile image)
        {
            string imageFolder = Directory.GetCurrentDirectory();
            
            string folder = imageFolder + Guid.NewGuid().ToString() + "_" + image.FileName;

            await image.CopyToAsync(new FileStream(Path.Combine(_webHostEnvironment.WebRootPath, folder), FileMode.Create));

            return "/" + folder;
        }*/
        public async Task<Guid> AddAlbumAsync(AlbumPostDto album)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !(user.Identity is { IsAuthenticated: true }))
            {
                throw new SecurityTokenException("User is not authenticated.");
            }
            
            var entity = _mapper.Map<Album>(album);
            if (!Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out Guid guidUserId))
                throw new InvalidOperationException("User ID is not a valid GUID.");
            
            entity.UserId = guidUserId;

            await _unitOfWork.Albums.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return entity.Id;
        }

        public async Task<AlbumWithElementsGetDto> GetAlbumById(Guid id)
        {
            var album = await _unitOfWork.Albums.GetByIdAsync(id);

            if (album == null)
                throw new NotFoundException("Album not found.");
            
            return _mapper.Map<AlbumWithElementsGetDto>(album);
        }


        /*
        public async Task<IEnumerable<AlbumGetDto>> GetAllAlbumsDTO(string? userId)
        {
            if (userId == null)
                throw new ArgumentNullException("userId is null");

            var albums = await _unitOfWork.Albums.GetAllAlbumsWithElements(userId);

            if (albums == null)
                throw new ArgumentNullException("albums is null");

            return _mapper.Map<List<AlbumGetDto>>(albums);
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

        public async Task<AlbumWithElementsGetDto> GetAlbumById(int? id)
        {
            if(id == null)
                throw new ArgumentNullException("id is null");

            var album = await _unitOfWork.Albums.GetAlbumWithElementsByIdAsync(id);

            if (album == null)
                throw new ArgumentNullException("album is null");

            return _mapper.Map<AlbumWithElementsGetDto>(album);
        }

        public async Task<AlbumEditDTO> GetAlbumForEdit(int? id, string? currentUserId)
        {
            if (id == null || currentUserId == null)
                throw new ArgumentNullException("id or currentUserId is null");

            var album = await _unitOfWork.Albums.GetAlbumWithAuthorCheckAsync(id, currentUserId);

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

            var album = await _unitOfWork.Albums.GetAlbumWithAuthorCheckAsync(id, currentUserId);

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
                    _unitOfWork.FavoriteAlbumsElement.Delete(user.FavoriteAlbums.FirstOrDefault(a => a.AlbumId == albumId));
                    album.Rate--;
                }

                await _unitOfWork.SaveChangesAsync();

            }

        }*/
    }
}
