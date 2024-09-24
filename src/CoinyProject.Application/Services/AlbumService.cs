using System.Security.Claims;
using AutoMapper;
using CoinyProject.Application.Abstractions.Repositories;
using CoinyProject.Application.Abstractions.Services;
using CoinyProject.Application.Dto.Album;
using CoinyProject.Application.DTO.Album;
using CoinyProject.Application.Dto.Other;
using CoinyProject.Domain.Entities;
using CoinyProject.Domain.Enums;
using CoinyProject.Domain.Exceptions;
using Microsoft.AspNetCore.Http;


namespace CoinyProject.Application.Services
{
    public class AlbumService : IAlbumService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

       public AlbumService(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
       {
           _mapper = mapper;
           _unitOfWork = unitOfWork;
           _httpContextAccessor = httpContextAccessor;
       }
    
        public async Task<Guid> AddAlbumAsync(AlbumPostDto album)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user is not { Identity.IsAuthenticated: true } || !Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                throw new UnauthorizedAccessException("User is not authenticated or user id is not valid");
            
            var entity = _mapper.Map<Album>(album);
            entity.UserId = userId;

            await _unitOfWork.Albums.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return entity.Id;
        }

        public async Task<PagedResponse<AlbumViewGetDto>> GetPagedAlbumsAsync(PageQueryDto pageQuery, SortByItemQueryDto? sortQuery)
        {
            var albums = await _unitOfWork.Albums.GetPagedActiveAlbumsWithElementsAsync(pageQuery,sortQuery);
            if (albums.TotalPages == 0)
                throw new NotFoundException("Any active album not found.");
           
            return _mapper.Map<PagedResponse<AlbumViewGetDto>>(albums);
        }

        public async Task<AlbumGetDto> GetAlbumById(Guid id)
        {
            var album = await _unitOfWork.Albums.GetByIdAsync(id);
            if (album == null)
                throw new NotFoundException("Album not found.");
            
            var user = _httpContextAccessor.HttpContext?.User;
            if (user is { Identity.IsAuthenticated: true })
            {
                Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);
                if(album.UserId != userId && album.Status != AlbumStatus.Active)
                    throw new UnauthorizedAccessException("User is not the owner of the album or album is not active");
            }
            else
            {
                if(album.Status != AlbumStatus.Active)
                    throw new UnauthorizedAccessException("Denied for not authenticated users or album is not active");
            }
            
            return _mapper.Map<AlbumGetDto>(album);
        }

        public async Task<Guid> UpdateAlbumAsync(Guid id, AlbumPatchDto album)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user is not { Identity.IsAuthenticated: true } || !Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                throw new UnauthorizedAccessException("User is not authenticated or user id is not valid");
            
            var oldAlbum = await _unitOfWork.Albums.GetByIdAsync(id);
            if (oldAlbum == null)
                throw new NotFoundException("Album not found.");
            if(oldAlbum.UserId != userId)
                throw new UnauthorizedAccessException("User is not the owner of the album");
            
            if (oldAlbum is { Status: AlbumStatus.Active })
                oldAlbum.Status = AlbumStatus.NotApproved;

            _mapper.Map(album, oldAlbum);
            await _unitOfWork.SaveChangesAsync();
            
            return id;
        }

        public async Task DeactivateAlbumAsync(Guid id)
        {
           await ChangeAlbumStatus(id, AlbumStatus.Active, AlbumStatus.Inactive);
        }

        public async Task ActivateAlbumAsync(Guid id)
        {
            var elements = await _unitOfWork.AlbumElements.GetPagedAlbumElementsByAlbumIdAsync(id, 1, 10);
            if(elements.Items.Count() < 4)
                throw new InvalidOperationException("Album should have at least 4 element to activate.");
            
            await ChangeAlbumStatus(id, AlbumStatus.Inactive, AlbumStatus.NotApproved);
        }

        public async Task ApproveAlbumAsync(Guid id)
        {
            var album = await _unitOfWork.Albums.GetByIdAsync(id);
            if (album == null)
                throw new NotFoundException("Album not found.");
            
            if (album.Status == AlbumStatus.NotApproved)
                album.Status = AlbumStatus.Active;
            
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task ChangeAlbumStatus(Guid id, AlbumStatus startStatus, AlbumStatus endStatus)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user is not { Identity.IsAuthenticated: true } || !Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                throw new UnauthorizedAccessException("User is not authenticated or user id is not valid");
            
            var album = await _unitOfWork.Albums.GetByIdAsync(id);
            if (album == null)
                throw new NotFoundException("Album not found.");
            if(album.UserId != userId)
                throw new UnauthorizedAccessException("User is not the owner of the album");
            
            if (album.Status == startStatus)
                album.Status = endStatus;
            
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
