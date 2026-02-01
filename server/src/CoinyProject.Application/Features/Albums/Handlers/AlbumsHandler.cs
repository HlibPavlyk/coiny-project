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
    IRequestHandler<DeactivateAlbumRequest, Result<Guid>>
{
    public async Task<Result<Paginated<AlbumModel>>> Handle(GetAlbumsRequest request, CancellationToken cancellationToken)
    {
        // For public: Active albums with >= 4 Approved elements
        var items = await GetAlbumItemsAsync(
            request,
            request.SortBy,
            x => x.Status == AlbumStatus.Active && x.Elements.Count(e => e.Status == AlbumElementStatus.Approved) >= 4,
            cancellationToken);

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
        var isOwner = currentUserId.IsSuccess && request.UserId == currentUserId.Value;

        // Owner sees all their albums, others see only Active + >= 4 Approved elements
        Expression<Func<Album, bool>> predicate = isOwner
            ? x => x.UserId == request.UserId
            : x => x.UserId == request.UserId &&
                   x.Status == AlbumStatus.Active &&
                   x.Elements.Count(e => e.Status == AlbumElementStatus.Approved) >= 4;

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
        var isOwner = currentUserId.IsSuccess && album.UserId == currentUserId.Value;

        if (!isOwner)
        {
            var approvedCount = album.Elements?.Count(e => e.Status == AlbumElementStatus.Approved) ?? 0;
            if (album.Status != AlbumStatus.Active || approvedCount < 4)
                return Result.Failure<AlbumModel>(Error.Forbidden("Album is not accessible"));
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
            Status = AlbumStatus.Active
        };

        await context.Albums.AddAsync(album, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return album.Id;
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

        // Status stays the same on update
        album.Name = request.Model.Name;
        album.Description = request.Model.Description;

        await context.SaveChangesAsync(cancellationToken);

        return album.Id;
    }

    public Task<Result<Guid>> Handle(ActivateAlbumRequest request, CancellationToken cancellationToken)
    {
        return ChangeAlbumStatusAsync(request.Id, AlbumStatus.Active, cancellationToken);
    }

    public Task<Result<Guid>> Handle(DeactivateAlbumRequest request, CancellationToken cancellationToken)
    {
        return ChangeAlbumStatusAsync(request.Id, AlbumStatus.Inactive, cancellationToken);
    }

    // Private helper methods
    private async Task<Result<Guid>> ChangeAlbumStatusAsync(
        Guid id,
        AlbumStatus status,
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

        if (album.Status != status)
        {
            album.Status = status;
            await context.SaveChangesAsync(cancellationToken);
        }

        return Result.Success(album.Id);
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
