using CoinyProject.Application.Common.Results;
using CoinyProject.Application.Features.Albums.Models;
using MediatR;

namespace CoinyProject.Application.Features.Albums.Requests;

public record CreateAlbumRequest : UpdateAlbumModel, IRequest<Result<Guid>>;