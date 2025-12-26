using CoinyProject.Application.Common.Models;
using CoinyProject.Application.Common.Results;
using CoinyProject.Application.Features.AlbumElements.Models;
using MediatR;

namespace CoinyProject.Application.Features.AlbumElements.Requests;

public record AddAlbumElementRequest(Guid AlbumId, UpdateAlbumElementModel Model, FileStreamDataModel File) : IRequest<Result<Guid>>;
