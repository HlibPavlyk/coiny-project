using CoinyProject.Application.Common.Results;
using MediatR;

namespace CoinyProject.Application.Features.Albums.Requests;

public record ApproveAlbumRequest(Guid Id) : IRequest<Result<Guid>>;
