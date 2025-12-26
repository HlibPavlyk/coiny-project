using CoinyProject.Application.Common.Results;
using MediatR;

namespace CoinyProject.Application.Features.AlbumElements.Requests;

public record DeleteAlbumElementRequest(Guid Id, Guid AlbumId) : IRequest<Result<Guid>>;
