using CoinyProject.Application.Common.Results;
using MediatR;

namespace CoinyProject.Application.Features.AlbumElements.Requests;

public record DeactivateAlbumElementRequest(Guid Id, Guid AlbumId) : IRequest<Result<Guid>>;
