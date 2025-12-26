using CoinyProject.Application.Abstractions.Data;
using CoinyProject.Application.Common.Results;
using CoinyProject.Application.Features.Users.Models;
using CoinyProject.Application.Features.Users.Requests;
using CoinyProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoinyProject.Application.Features.Users.Handlers;

public class UsersHandler(IApplicationDbContext context) :
    IRequestHandler<GetUserProfileRequest, Result<UserProfileModel>>
{
    public async Task<Result<UserProfileModel>> Handle(GetUserProfileRequest request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .AsNoTracking()
            .Include(u => u.Albums.Where(a => a.Status == AlbumStatus.Active))
            .ThenInclude(a => a.Elements)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
            return Result.Failure<UserProfileModel>(Error.NotFound("User not found"));

        return new UserProfileModel
        {
            Username = user.UserName,
            DiscussionRate = user.DiscussionRate,
            LikesCount = user.Albums.Sum(a => a.Rate),
            AlbumsCount = user.Albums.Count,
            AlbumsElementsCount = user.Albums.SelectMany(a => a.Elements).Count()
        };
    }
}
