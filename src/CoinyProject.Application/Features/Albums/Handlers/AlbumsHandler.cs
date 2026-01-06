using System.Linq.Expressions;
using AutoMapper;
using CoinyProject.Application.Abstractions.Data;
using CoinyProject.Application.Abstractions.Identity;
using CoinyProject.Application.Abstractions.Querying;
using CoinyProject.Application.Common.Extensions;
using CoinyProject.Application.Common.Models;
using CoinyProject.Application.Common.Querying;
using CoinyProject.Application.Common.Results;
using CoinyProject.Application.Features.Albums.Models;
using CoinyProject.Application.Features.Albums.Requests;
using CoinyProject.Domain.Entities;
using CoinyProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoinyProject.Application.Features.Albums.Handlers;

public class AlbumsHandler(IApplicationDbContext context, IMapper mapper, IIdentityService identityService) :
    IRequestHandler<GetAlbumsRequest, Result<Paginated<AlbumModel>>>,
    IRequestHandler<GetUserAlbumsRequest, Result<Paginated<AlbumModel>>>,
    IRequestHandler<GetAlbumByIdRequest, Result<AlbumModel>>,
    IRequestHandler<CreateAlbumRequest, Result<Guid>>,
    IRequestHandler<UpdateAlbumRequest, Result<Guid>>,
    IRequestHandler<ActivateAlbumRequest, Result<Guid>>,
    IRequestHandler<DeactivateAlbumRequest, Result<Guid>>,
    IRequestHandler<ApproveAlbumRequest, Result<Guid>>
{
    public async Task<Result<Paginated<AlbumModel>>> Handle(GetAlbumsRequest request, CancellationToken cancellationToken)
    {
        var items = await GetAlbumItemsAsync(request, request.SortBy, x => x.Status == AlbumStatus.Active, cancellationToken);

        return new Paginated<AlbumModel>
        {
            TotalCount = items.Length,
            Items = items.Paginate(request).ToArray(),
        };
    }
    
    public async Task<Result<Paginated<AlbumModel>>> Handle(
        GetUserAlbumsRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = identityService.GetCurrentUserId();
        Expression<Func<Album, bool>> predicate = currentUserId.IsSuccess && request.UserId != currentUserId.Value
            ? x => x.Status == AlbumStatus.Active
            : null;
        
        var items = await GetAlbumItemsAsync(request.Paginate, request.Paginate.SortBy, predicate, cancellationToken);

        return new Paginated<AlbumModel>
        {
            TotalCount = items.Length,
            Items = items.Paginate(request.Paginate).ToArray(),
        };
    }

    public async Task<Result<AlbumModel>> Handle(GetAlbumByIdRequest request, CancellationToken cancellationToken)
    {
        var album = await context.Albums
            .Include(x => x.User)
            .Include(x => x.Elements)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (album is null)
            return Result.Failure<AlbumModel>(Error.NotFound("Album not found"));

        var currentUserId = identityService.GetCurrentUserId();
        if (currentUserId.IsSuccess)
        {
            if (album.UserId != currentUserId.Value && album.Status != AlbumStatus.Active)
                return Result.Failure<AlbumModel>(Error.Forbidden("User is not the owner of the album or album is not active"));
        }
        else
        {
            if (album.Status != AlbumStatus.Active)
                return Result.Failure<AlbumModel>(Error.Forbidden("Album is not active"));
        }

        return MapToAlbumModel(album);
    }

    public async Task<Result<Guid>> Handle(CreateAlbumRequest request, CancellationToken cancellationToken)
    {
        var currentUserId = identityService.GetCurrentUserId();
        if (currentUserId.IsFailure)
            return Result.Failure<Guid>(currentUserId.Error);

        var album = new Album
        {
            Name = request.Name,
            Description = request.Description,
            UserId = currentUserId.Value,
            Status = AlbumStatus.Inactive
        };

        await context.Albums.AddAsync(album, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return Guid.NewGuid();
    }

    public async Task<Result<Guid>> Handle(UpdateAlbumRequest request, CancellationToken cancellationToken)
    {
        var userIdResult = identityService.GetCurrentUserId();
        if (userIdResult.IsFailure)
            return Result.Failure<Guid>(userIdResult.Error);

        var album = await context.Albums.FindAsync([request.Id], cancellationToken);
        if (album is null)
            return Result.Failure<Guid>(Error.NotFound("Album not found"));

        if (album.UserId != userIdResult.Value)
            return Result.Failure<Guid>(Error.Forbidden("User is not the owner of the album"));

        if (album.Status == AlbumStatus.Active)
            album.Status = AlbumStatus.NotApproved;

        album.Name = request.Model.Name;
        album.Description = request.Model.Description;

        await context.SaveChangesAsync(cancellationToken);

        return album.Id;
    }

    public async Task<Result<Guid>> Handle(ActivateAlbumRequest request, CancellationToken cancellationToken)
    {
        var elementCount = await context.AlbumElements.CountAsync(x => x.AlbumId == request.Id, cancellationToken);
        if (elementCount < 4)
            return Result.Failure<Guid>(Error.Validation("Album should have at least 4 elements to activate"));

        return await ChangeAlbumStatusAsync(request.Id, AlbumStatus.NotApproved, AlbumStatus.Inactive, cancellationToken);
    }

    public async Task<Result<Guid>> Handle(DeactivateAlbumRequest request, CancellationToken cancellationToken)
    {
        return await ChangeAlbumStatusAsync(request.Id, AlbumStatus.Inactive, null, cancellationToken);
    }

    public async Task<Result<Guid>> Handle(ApproveAlbumRequest request, CancellationToken cancellationToken)
    {
        var album = await context.Albums.FindAsync([request.Id], cancellationToken);
        if (album is null)
            return Result.Failure<Guid>(Error.NotFound("Album not found"));

        if (album.Status == AlbumStatus.NotApproved)
            album.Status = AlbumStatus.Active;

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success(album.Id);
    }

    // Private helper methods
    private async Task<Result<Guid>> ChangeAlbumStatusAsync(
        Guid id,
        AlbumStatus endStatus,
        AlbumStatus? startStatus,
        CancellationToken cancellationToken)
    {
        var userIdResult = identityService.GetCurrentUserId();
        if (userIdResult.IsFailure)
            return Result.Failure<Guid>(userIdResult.Error);

        var album = await context.Albums.FindAsync([id], cancellationToken);
        if (album is null)
            return Result.Failure<Guid>(Error.NotFound("Album not found"));

        if (album.UserId != userIdResult.Value)
            return Result.Failure<Guid>(Error.Forbidden("User is not the owner of the album"));

        if (startStatus is null || album.Status == startStatus)
            album.Status = endStatus;

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success(id);
    }

    private async Task<AlbumModel[]> GetAlbumItemsAsync(
        ITextSearch search,
        SortByModel[] sortBy,
        Expression<Func<Album, bool>> predicate,
        CancellationToken cancellationToken)
    {
        var query = context.Albums
            .Include(x => x.User)
            .Include(x => x.Elements)
            .AsNoTracking();

        if (predicate is not null)
            query = query.Where(predicate);

        var items = await query
            .AsAsyncEnumerable()
            .Search(
                search,
                album => album.Name,
                album => album.Status,
                album => album.Description,
                album => album.Rate,
                album => album.CreatedAt,
                album => album.UpdatedAt
            )
            .Select(MapToAlbumModel)
            .ToArrayAsync(cancellationToken);

        return items.SortBy(sortBy).ToArray();
    }

    private AlbumModel MapToAlbumModel(Album album)
    {
        return new AlbumModel
        {
            Id = album.Id,
            Name = album.Name,
            Description = album.Description,
            Rate = album.Rate,
            Status = album.Status,
            Author = mapper.Map<BaseLink>(album.User),
            Images = album.Elements?.Select(mapper.Map<AlbumElementLink>).ToArray() ?? [],
            CreatedAt = album.CreatedAt,
            UpdatedAt = album.UpdatedAt,
        };
    }
}
