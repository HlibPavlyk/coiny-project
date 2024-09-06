using CoinyProject.Application.Abstractions.Services;
using CoinyProject.Application.Dto.Album;
using CoinyProject.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

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
            return Created($"/api/albums/{id}", id);
        }
        catch (ArgumentNullException e)
        {
            return BadRequest(e.Message);
        }
        catch (SecurityTokenException e)
        {
            return Unauthorized(new { Message = "Invalid or expired token." });
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
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}