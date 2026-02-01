using CoinyProject.Application.Common.Results;
using CoinyProject.Application.Features.AlbumElements.Models;
using MediatR;

namespace CoinyProject.Application.Features.AlbumElements.Requests;

public record GetAlbumElementByIdRequest(Guid Id, Guid AlbumId) : IRequest<Result<AlbumElementModel>>;
