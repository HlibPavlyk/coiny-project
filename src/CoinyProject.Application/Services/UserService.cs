using System.Security.Claims;
using AutoMapper;
using CoinyProject.Application.Abstractions.Repositories;
using CoinyProject.Application.Abstractions.Services;
using CoinyProject.Application.Dto.Album;
using CoinyProject.Application.Dto.User;
using CoinyProject.Domain.Entities;
using CoinyProject.Domain.Exceptions;
using Microsoft.AspNetCore.Http;

namespace CoinyProject.Application.Services;

public class UserService : IUserService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }
  
    public async Task<UserStatsGetDto> GetUserStatsAsync(Guid userId)
    {
       var user = await _unitOfWork.Users.GetUserWithInfoForStatsAsync(userId);
       if (user == null)
           throw new NotFoundException("User not found.");
       
       return _mapper.Map<UserStatsGetDto>(user);
    }

    public async Task<UserStatsGetDto> GetCurrentUserStatsAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user is not { Identity.IsAuthenticated: true } || !Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var currentUserId))
            throw new UnauthorizedAccessException("User is not authenticated or user id is not valid");
        
        return await GetUserStatsAsync(currentUserId);
    }
}