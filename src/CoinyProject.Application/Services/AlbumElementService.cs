using System.Security.Claims;
using AutoMapper;
using CoinyProject.Application.Abstractions.DataServices;
using CoinyProject.Application.Abstractions.Repositories;
using CoinyProject.Application.Abstractions.Services;
using CoinyProject.Application.Dto.AlbumElement;
using CoinyProject.Application.Dto.Other;
using CoinyProject.Domain.Entities;
using CoinyProject.Domain.Enums;
using CoinyProject.Domain.Exceptions;
using Microsoft.AspNetCore.Http;

namespace CoinyProject.Application.Services;

public class AlbumElementService : IAlbumElementService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileService _fileService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AlbumElementService(IMapper mapper, IUnitOfWork unitOfWork, IFileService fileService,
        IHttpContextAccessor httpContextAccessor)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _fileService = fileService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Guid> AddAlbumElement(AlbumElementPostDto element)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user is not { Identity.IsAuthenticated: true } || !Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            throw new UnauthorizedAccessException("User is not authenticated or user id is not valid");
        
        if (!await _unitOfWork.Albums.AnyAsync(x => x.Id == element.AlbumId))
            throw new ArgumentNullException(nameof(element.AlbumId), "AlbumId is null");
        
        var album = await _unitOfWork.Albums.GetByIdAsync(element.AlbumId);
        if( album != null && album.UserId != userId)
            throw new UnauthorizedAccessException("User is not the owner of the album");

        var entity = _mapper.Map<AlbumElement>(element);
        entity.ImageUrl = await _fileService.SaveImageAsync(element.Photo);

        if (album is { Status: AlbumStatus.Active })
            album.Status = AlbumStatus.NotApproved;
        
        await _unitOfWork.AlbumElements.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        
        return entity.Id;
    }

    public async Task<PagedResponse<AlbumElementGetDto>> GetPagedAlbumElementsByAlbumIdAsync(Guid id, PageQueryDto pageQuery, SortByItemQueryDto? sortQuery, string? search)
    {
        if (!await _unitOfWork.Albums.AnyAsync(x => x.Id == id))
            throw new ArgumentNullException(nameof(id), "AlbumId is null");
        
        var elements = await _unitOfWork.AlbumElements.GetPagedAlbumElementsByAlbumIdAsync(id, pageQuery, sortQuery, search);
        if (elements.TotalPages == 0)
            throw new NotFoundException("No elements found for this album");
        
        var user = _httpContextAccessor.HttpContext?.User;
        var album = await _unitOfWork.Albums.GetByIdAsync(id);
        
        if (user is { Identity.IsAuthenticated: true })
        {
            Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);
            if( album != null && album.UserId != userId && album.Status != AlbumStatus.Active)
                throw new UnauthorizedAccessException("User is not the owner of the album or album is not active");
        }
        else
        {
            if( album != null && album.Status != AlbumStatus.Active)
                throw new UnauthorizedAccessException("Denied for not authenticated users or album is not active");
        }
        
        return _mapper.Map<PagedResponse<AlbumElementGetDto>>(elements);
    }

    public async Task<AlbumElementViewGetDto> GetAlbumElementByIdAsync(Guid id)
    {
        var element = await _unitOfWork.AlbumElements.GetAlbumElementWithAlbumByIdAsync(id);
        if (element == null)
            throw new NotFoundException("Element not found");
        
        var user = _httpContextAccessor.HttpContext?.User;
        var album = await _unitOfWork.Albums.GetByIdAsync(element.AlbumId);

        if (user is { Identity.IsAuthenticated: true })
        {
            Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId);
            if( album != null && album.UserId != userId && album.Status != AlbumStatus.Active)
                throw new UnauthorizedAccessException("User is not the owner of the album or album is not active");
        }
        else
        {
            if( album != null && album.Status != AlbumStatus.Active)
                throw new UnauthorizedAccessException("Denied for not authenticated users or album is not active");
        }
        
        return _mapper.Map<AlbumElementViewGetDto>(element);
    }

    public async Task<Guid> UpdateAlbumElementAsync(Guid id, AlbumElementPatchDto element)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user is not { Identity.IsAuthenticated: true } || !Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            throw new UnauthorizedAccessException("User is not authenticated or user id is not valid");
        
        var oldElement = await _unitOfWork.AlbumElements.GetByIdAsync(id);
        if (oldElement == null)
            throw new NotFoundException("Element not found");
        
        var album = await _unitOfWork.Albums.GetByIdAsync(oldElement.AlbumId);
        if( album != null && album.UserId != userId)
            throw new UnauthorizedAccessException("User is not the owner of the album");
        
        _mapper.Map(element, oldElement);
        if (element.Photo != null)
            oldElement.ImageUrl = await _fileService.SaveImageAsync(element.Photo);
        
        if (album is { Status: AlbumStatus.Active })
            album.Status = AlbumStatus.NotApproved;
        
        await _unitOfWork.SaveChangesAsync();
        return id;
    }

    public async Task DeleteAlbumElementAsync(Guid id)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user is not { Identity.IsAuthenticated: true } || !Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            throw new UnauthorizedAccessException("User is not authenticated or user id is not valid");
        
        var element = await _unitOfWork.AlbumElements.GetByIdAsync(id);
        if (element == null)
            throw new NotFoundException("Element not found");
        
        var album = await _unitOfWork.Albums.GetByIdAsync(element.AlbumId);
        if( album != null && album.UserId != userId)
            throw new UnauthorizedAccessException("User is not the owner of the album");
        
        _unitOfWork.AlbumElements.Remove(element);
        await _unitOfWork.SaveChangesAsync();
    }
}