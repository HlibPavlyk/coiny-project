using CoinyProject.Api.Responses;
using CoinyProject.Application.Abstractions.Services;
using CoinyProject.Application.Dto.AlbumElement;
using CoinyProject.Application.Dto.Other;
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
            return CreatedAtAction(nameof(GetAlbumElementById), new { id },
                await _albumElementService.GetAlbumElementByIdAsync(id));
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

    [HttpGet("by-album/{albumId:guid}")]
    public async Task<IActionResult> GetPagedAlbumElementsByAlbumIdAsync([FromRoute] Guid albumId,[FromQuery] int page = 1,[FromQuery] int size = 10,
        [FromQuery] string sortItem = "time", [FromQuery] bool isAscending = false, [FromQuery] string? search = null)
    {
        try
        {
            var elements = await _albumElementService.GetPagedAlbumElementsByAlbumIdAsync(albumId,
                new PageQueryDto(page, size), new SortByItemQueryDto(sortItem, isAscending), search);
            return Ok(elements);
        }
        catch (UnauthorizedAccessException e)
        {
            return new CustomForbidResult(e.Message);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (ArgumentNullException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAlbumElementById([FromRoute] Guid id)
    {
        try
        {
            var element = await _albumElementService.GetAlbumElementByIdAsync(id);
            return Ok(element);
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
    public async Task<IActionResult> UpdateAlbumElement([FromRoute] Guid id, [FromForm] AlbumElementPatchDto element)
    {
        try
        {
            await _albumElementService.UpdateAlbumElementAsync(id, element);
            return Ok(await _albumElementService.GetAlbumElementByIdAsync(id));
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
    
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteAlbumElement([FromRoute] Guid id)
    {
        try
        {
            await _albumElementService.DeleteAlbumElementAsync(id);
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