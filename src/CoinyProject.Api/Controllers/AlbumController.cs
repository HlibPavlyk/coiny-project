using CoinyProject.Api.Responses;
using CoinyProject.Application.Abstractions.Services;
using CoinyProject.Application.Dto.Album;
using CoinyProject.Application.DTO.Album;
using CoinyProject.Application.Dto.Other;
using CoinyProject.Application.Models;
using CoinyProject.Application.Requests;
using CoinyProject.Application.Requests.Albums;
using CoinyProject.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoinyProject.Api.Controllers;

[ApiController]
[Route("api/albums")]
public class AlbumController(IAlbumService albumService, IMediator mediator) : Controller
{

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddAlbum([FromBody] AlbumPostDto album)
    {
        try
        {
            var id = await albumService.AddAlbumAsync(album);
            return CreatedAtAction(nameof(GetAlbumById), new { id }, await albumService.GetAlbumById(id));
        }
        catch (UnauthorizedAccessException e)
        {
            return new CustomForbidResult(e.Message);
        }
        catch (ArgumentNullException e)
        {
            return BadRequest(e.Message);
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> GetPagedAlbums([FromQuery] int page = 1,[FromQuery] int size = 10,
        [FromQuery] string sortItem = "time", [FromQuery] bool isAscending = false, [FromQuery] string? search = null)
    {
        try
        {
            var albums = await albumService.GetPagedAlbumsAsync(new PageQueryDto(page, size), new SortByItemQueryDto(sortItem, isAscending), search);
            return Ok(albums);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
    }
    
    [HttpPost("search")]
    public Task<PaginatedItemsModel<AlbumViewGetDto>> GetAlbumItems(GetAlbumItemsRequest request, CancellationToken cancellationToken = default)
    {
        return mediator.Send(request, cancellationToken);
    }
    
    [HttpGet("by-user")]
    public async Task<IActionResult> GetPagedAlbums([FromQuery] Guid? userId, [FromQuery] int page = 1, [FromQuery] int size = 10,
        [FromQuery] string sortItem = "time", [FromQuery] bool isAscending = false, [FromQuery] string? search = null)
    {
        try
        {
            if (userId.HasValue)
            {
                var albums = await albumService.GetPagedActiveAlbumsByUserIdAsync(userId.Value, new PageQueryDto(page, size),
                    new SortByItemQueryDto(sortItem, isAscending), search);
                return Ok(albums);
            }
            else
            {
                var albums = await albumService.GetCurrentUserPagedAlbumsAsync(new PageQueryDto(page, size),
                    new SortByItemQueryDto(sortItem, isAscending), search);
                return Ok(albums);
            }
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (UnauthorizedAccessException e)
        {
            return BadRequest(e.Message);
        }
    }

    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAlbumById([FromRoute] Guid id)
    {
        try
        {
            var album = await albumService.GetAlbumById(id);
            return Ok(album);
        }
        catch (UnauthorizedAccessException e)
        {
            return new CustomForbidResult(e.Message);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
    
    [HttpPatch("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> UpdateAlbum([FromRoute] Guid id, [FromBody] AlbumPatchDto album)
    {
        try
        {
            await albumService.UpdateAlbumAsync(id, album);
            return Ok(await albumService.GetAlbumById(id));
        }
        catch (UnauthorizedAccessException e)
        {
            return new CustomForbidResult(e.Message);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
    
    [HttpPost("{id:guid}/deactivate")]
    [Authorize]
    public async Task<IActionResult> DeactivateAlbum([FromRoute] Guid id)
    {
        try
        {
            await albumService.DeactivateAlbumAsync(id);
            return NoContent();
        }
        catch (UnauthorizedAccessException e)
        {
            return new CustomForbidResult(e.Message);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
    
    [HttpPost("{id:guid}/activate")]
    [Authorize]
    public async Task<IActionResult> ActivateAlbum([FromRoute] Guid id)
    {
        try
        {
            await albumService.ActivateAlbumAsync(id);
            return NoContent();
        }
        catch (UnauthorizedAccessException e)
        {
            return new CustomForbidResult(e.Message);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(e.Message);
        }
    }
    
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> ApproveAlbum([FromRoute] Guid id)
    {
        try
        {
            await albumService.ApproveAlbumAsync(id);
            return NoContent();
        }
        catch (UnauthorizedAccessException e)
        {
            return new CustomForbidResult(e.Message);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}