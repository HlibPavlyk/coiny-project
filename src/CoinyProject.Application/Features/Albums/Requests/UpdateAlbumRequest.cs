using CoinyProject.Application.Common.Results;
using CoinyProject.Application.Features.Albums.Models;
using MediatR;

namespace CoinyProject.Application.Features.Albums.Requests;

public record UpdateAlbumRequest(Guid Id, UpdateAlbumModel Model) : IRequest<Result<Guid>>;
