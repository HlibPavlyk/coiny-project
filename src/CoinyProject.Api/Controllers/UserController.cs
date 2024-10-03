using CoinyProject.Application.Abstractions.Services;
using CoinyProject.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CoinyProject.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : Controller
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetUser([FromQuery] Guid? id)
    {
        try
        {
            if (id == null)
            {
                var currentUser = await _userService.GetCurrentUserStatsAsync();
                return Ok(currentUser);
            }

            var user = await _userService.GetUserStatsAsync(id.Value);
            return Ok(user);
        }
        catch (UnauthorizedAccessException e)
        {
            return BadRequest(e.Message);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

}