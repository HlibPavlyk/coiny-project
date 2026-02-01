using CoinyProject.Application.Common.Results;
using MediatR;

namespace CoinyProject.Application.Features.Albums.Requests;

public record DeactivateAlbumRequest(Guid Id) : IRequest<Result<Guid>>;
