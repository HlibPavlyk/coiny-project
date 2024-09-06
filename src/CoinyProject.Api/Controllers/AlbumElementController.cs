using CoinyProject.Application.Abstractions.Services;
using CoinyProject.Application.Dto.Album;
using CoinyProject.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoinyProject.Api.Controllers;

[ApiController]
[Route("api/album-elements")]
public class AlbumElementController : Controller
{
    private readonly IAlbumElementService _albumElementService;

    public AlbumElementController(IAlbumElementService albumElementService)
    {
        _albumElementService = albumElementService;
    }
    
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddAlbumElement([FromForm] AlbumElementPostDto element)
    {
        try
        {
            var id = await _albumElementService.AddAlbumElement(element);
            return Ok(id);
        }
        catch (ArgumentNullException e)
        {
            return BadRequest(e.Message);
        }
    }
    
    [HttpGet("{albumId:guid}")]
    public async Task<IActionResult> GetPagedAlbumElementsByAlbumIdAsync([FromRoute]Guid albumId, 
        [FromQuery]int page = 1, int size = 10)
    {
        try
        {
            var elements = await _albumElementService.GetPagedAlbumElementsByAlbumIdAsync(albumId, page, size);
            return Ok(elements);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (ArgumentNullException e)
        {
            return NotFound(e.Message);
        }
    }
}