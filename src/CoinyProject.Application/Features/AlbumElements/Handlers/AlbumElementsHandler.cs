using AutoMapper;
using CoinyProject.Application.Abstractions.Data;
using CoinyProject.Application.Abstractions.Identity;
using CoinyProject.Application.Common.Extensions;
using CoinyProject.Application.Common.Querying;
using CoinyProject.Application.Common.Requests;
using CoinyProject.Application.Common.Results;
using CoinyProject.Application.Features.AlbumElements.Models;
using CoinyProject.Application.Features.AlbumElements.Requests;
using CoinyProject.Domain.Entities;
using CoinyProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoinyProject.Application.Features.AlbumElements.Handlers;

public class AlbumElementsHandler(IApplicationDbContext context, IIdentityService identityService, IFileService fileService, IMapper mapper) :
    IRequestHandler<AddAlbumElementRequest, Result<Guid>>,
    IRequestHandler<UpdateAlbumElementRequest, Result<Guid>>,
    IRequestHandler<GetAlbumElementsRequest, Result<Paginated<AlbumElementListItemModel>>>,
    IRequestHandler<GetAlbumElementByIdRequest, Result<AlbumElementModel>>,
    IRequestHandler<DeleteAlbumElementRequest, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AddAlbumElementRequest request, CancellationToken cancellationToken)
    {
        var userIdResult = identityService.GetCurrentUserId();
        if (userIdResult.IsFailure)
            return Result.Failure<Guid>(userIdResult.Error);

        var album = await context.Albums.FindAsync([request.AlbumId], cancellationToken);
        if (album is null)
            return Result.Failure<Guid>(Error.NotFound("Album not found"));

        if (album.UserId != userIdResult.Value)
            return Result.Failure<Guid>(Error.Forbidden("User is not the owner of the album"));

        var entity = new AlbumElement
        {
            Name = request.Model.Name,
            Description = request.Model.Description,
            AlbumId = request.AlbumId,
            ImageUrl = await fileService.SaveImageAsync(request.File)
        };

        if (album.Status == AlbumStatus.Active)
            album.Status = AlbumStatus.NotApproved;

        await context.AlbumElements.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    public async Task<Result<Paginated<AlbumElementListItemModel>>> Handle(GetAlbumElementsRequest request, CancellationToken cancellationToken)
    {
        var album = await context.Albums.FindAsync([request.AlbumId], cancellationToken);
        if (album is null)
            return Result.Failure<Paginated<AlbumElementListItemModel>>(Error.NotFound("Album not found"));

        var accessResult = CheckAlbumAccess(album);
        if (accessResult.IsFailure)
            return Result.Failure<Paginated<AlbumElementListItemModel>>(accessResult.Error);

        var items = await GetAlbumElementItemsAsync(request.AlbumId, request.Paginate, cancellationToken);

        return new Paginated<AlbumElementListItemModel>
        {
            TotalCount = items.Length,
            Items = items.Paginate(request.Paginate).ToArray()
        };
    }

    public async Task<Result<AlbumElementModel>> Handle(GetAlbumElementByIdRequest request, CancellationToken cancellationToken)
    {
        var element = await context.AlbumElements
            .Include(x => x.Album)
            .ThenInclude(x => x.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (element is null)
            return Result.Failure<AlbumElementModel>(Error.NotFound("Element not found"));

        var accessResult = CheckAlbumAccess(element.Album);
        if (accessResult.IsFailure)
            return Result.Failure<AlbumElementModel>(accessResult.Error);

        return mapper.Map<AlbumElementModel>(element);
    }

    public async Task<Result<Guid>> Handle(UpdateAlbumElementRequest request, CancellationToken cancellationToken)
    {
        var userIdResult = identityService.GetCurrentUserId();
        if (userIdResult.IsFailure)
            return Result.Failure<Guid>(userIdResult.Error);

        var existingElement = await context.AlbumElements.FindAsync([request.Id], cancellationToken);
        if (existingElement is null)
            return Result.Failure<Guid>(Error.NotFound("Element not found"));

        var album = await context.Albums.FindAsync([existingElement.AlbumId], cancellationToken);
        if (album is null)
            return Result.Failure<Guid>(Error.NotFound("Album not found"));

        if (album.UserId != userIdResult.Value)
            return Result.Failure<Guid>(Error.Forbidden("User is not the owner of the album"));

        if (request.Model?.Name is not null)
            existingElement.Name = request.Model.Name;

        if (request.Model?.Description is not null)
            existingElement.Description = request.Model.Description;

        if (request.File is not null)
            existingElement.ImageUrl = await fileService.SaveImageAsync(request.File);

        if (album.Status == AlbumStatus.Active)
            album.Status = AlbumStatus.NotApproved;

        await context.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    public async Task<Result<Guid>> Handle(DeleteAlbumElementRequest request, CancellationToken ct)
    {
        var userIdResult = identityService.GetCurrentUserId();
        if (userIdResult.IsFailure)
            return Result.Failure<Guid>(userIdResult.Error);

        var element = await context.AlbumElements.FindAsync([request.Id], ct);
        if (element is null)
            return Result.Failure<Guid>(Error.NotFound("Element not found"));

        var album = await context.Albums.FindAsync([request.AlbumId], ct);
        if (album is null)
            return Result.Failure<Guid>(Error.NotFound("Album not found"));

        if (album.UserId != userIdResult.Value)
            return Result.Failure<Guid>(Error.Forbidden("User is not the owner of the album"));

        context.AlbumElements.Remove(element);
        await context.SaveChangesAsync(ct);

        return request.Id;
    }
    
    private async Task<AlbumElementListItemModel[]> GetAlbumElementItemsAsync(
        Guid albumId,
        GetPaginatedItemsBaseRequest search,
        CancellationToken cancellationToken)
    {
        var items = await context.AlbumElements
            .Where(x => x.AlbumId == albumId)
            .AsNoTracking()
            .AsAsyncEnumerable()
            .Search(
                search,
                element => element.Name,
                element => element.Description,
                element => element.Rate,
                element => element.UpdatedAt)
            .Select(MapToListItemModel)
            .ToArrayAsync(cancellationToken);

        return items.SortBy(search?.SortBy).ToArray();
    }

    private AlbumElementListItemModel MapToListItemModel(AlbumElement element)
    {
        return new AlbumElementListItemModel
        {
            Id = element.Id,
            Name = element.Name,
            Description = element.Description,
            Rate = element.Rate,
            ImageUrl = fileService.GetImageUrl(element.ImageUrl),
            UpdatedAt = element.UpdatedAt
        };
    }

    private Result CheckAlbumAccess(Album album)
    {
        var userIdResult = identityService.GetCurrentUserId();

        if (userIdResult.IsSuccess)
        {
            if (album.UserId != userIdResult.Value && album.Status != AlbumStatus.Active)
                return Result.Failure(Error.Forbidden("User is not the owner of the album or album is not active"));
        }
        else
        {
            if (album.Status != AlbumStatus.Active)
                return Result.Failure(Error.Forbidden("Album is not active"));
        }

        return Result.Success();
    }
}
