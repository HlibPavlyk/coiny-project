using AutoMapper;
using CoinyProject.Application.Abstractions.DataServices;
using CoinyProject.Application.Abstractions.Repositories;
using CoinyProject.Application.Abstractions.Services;
using CoinyProject.Application.Dto.Album;
using CoinyProject.Application.DTO.Album;
using CoinyProject.Application.Dto.Other;
using CoinyProject.Domain.Entities;
using CoinyProject.Domain.Exceptions;

namespace CoinyProject.Application.Services;

public class AlbumElementService : IAlbumElementService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileService _fileService;

    public AlbumElementService(IMapper mapper, IUnitOfWork unitOfWork, IFileService fileService)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _fileService = fileService;
    }

    public async Task<Guid> AddAlbumElement(AlbumElementPostDto element)
    {
        if (!await _unitOfWork.Albums.AnyAsync(x => x.Id == element.AlbumId))
            throw new ArgumentNullException(nameof(element.AlbumId), "AlbumId is null");

        var entity = _mapper.Map<AlbumElement>(element);
        entity.ImageUrl = await _fileService.SaveImageAsync(element.Photo);
        
        await _unitOfWork.AlbumElements.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        
        return entity.Id;
    }

    public async Task<PagedResponse<AlbumElementGetDto>> GetPagedAlbumElementsByAlbumIdAsync(Guid id, int page, int size)
    {
        if (!await _unitOfWork.Albums.AnyAsync(x => x.Id == id))
            throw new ArgumentNullException(nameof(id), "AlbumId is null");

        var elements = await _unitOfWork.AlbumElements.GetPagedAlbumElementsByAlbumIdAsync(id, page, size);
        if (elements.TotalPages == 0)
        {
            throw new NotFoundException("No elements found for this album");
        }
        
        return _mapper.Map<PagedResponse<AlbumElementGetDto>>(elements);
    }
}