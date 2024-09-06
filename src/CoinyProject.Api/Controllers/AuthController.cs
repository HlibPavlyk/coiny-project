using System.Security.Authentication;
using CoinyProject.Application.Abstractions.Services;
using CoinyProject.Application.DTO.Auth;
using Microsoft.AspNetCore.Mvc;

namespace CoinyProject.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        try
        {
            var id = await _authService.RegisterUserAsync(registerDto);
            return Created($"/api/auth/{id}", id);
        }
        catch (AuthenticationException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
    {
        try
        {
            var token = await _authService.LoginAsync(loginDto);
            return Ok(token);
        }
        catch (AuthenticationException e)
        {
            return BadRequest(e.Message);
        }
    }
}