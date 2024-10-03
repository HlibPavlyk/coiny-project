using CoinyProject.Api.Responses;
using CoinyProject.Application.Abstractions.Services;
using CoinyProject.Application.Dto.Album;
using CoinyProject.Application.Dto.Other;
using CoinyProject.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoinyProject.Api.Controllers;

[ApiController]
[Route("api/albums")]
public class AlbumController : Controller
{
    private readonly IAlbumService _albumService;

    public AlbumController(IAlbumService albumService)
    {
        _albumService = albumService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddAlbum([FromBody] AlbumPostDto album)
    {
        try
        {
            var id = await _albumService.AddAlbumAsync(album);
            return CreatedAtAction(nameof(GetAlbumById), new { id }, await _albumService.GetAlbumById(id));
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
        [FromQuery] string sortItem = "time", [FromQuery] bool isAscending = false)
    {
        try
        {
            var albums = await _albumService.GetPagedAlbumsAsync(new PageQueryDto(page, size), new SortByItemQueryDto(sortItem, isAscending));
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
    
    [HttpGet("by-user")]
    [Authorize]
    public async Task<IActionResult> GetPagedAlbums([FromQuery] Guid? userId, [FromQuery] int page = 1, [FromQuery] int size = 10,
        [FromQuery] string sortItem = "time", [FromQuery] bool isAscending = false)
    {
        try
        {
            if (userId.HasValue)
            {
                var albums = await _albumService.GetPagedActiveAlbumsByUserIdAsync(userId.Value, new PageQueryDto(page, size),
                    new SortByItemQueryDto(sortItem, isAscending));
                return Ok(albums);
            }
            else
            {
                var albums = await _albumService.GetCurrentUserPagedAlbumsAsync(new PageQueryDto(page, size),
                    new SortByItemQueryDto(sortItem, isAscending));
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
            var album = await _albumService.GetAlbumById(id);
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
            await _albumService.UpdateAlbumAsync(id, album);
            return Ok(await _albumService.GetAlbumById(id));
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
            await _albumService.DeactivateAlbumAsync(id);
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
            await _albumService.ActivateAlbumAsync(id);
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
            await _albumService.ApproveAlbumAsync(id);
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