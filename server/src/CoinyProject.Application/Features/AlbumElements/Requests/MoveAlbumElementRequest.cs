using CoinyProject.Application.Common.Results;
using CoinyProject.Application.Features.AlbumElements.Models;
using MediatR;

namespace CoinyProject.Application.Features.AlbumElements.Requests;

public record MoveAlbumElementRequest(Guid Id, Guid AlbumId, MoveAlbumElementModel Model) : IRequest<Result<Guid>>;
