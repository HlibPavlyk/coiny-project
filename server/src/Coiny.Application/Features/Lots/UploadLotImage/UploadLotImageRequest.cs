using Coiny.Application.Common.Results;
using Coiny.Application.Features.Lots.Models;
using MediatR;

namespace Coiny.Application.Features.Lots.UploadLotImage;

/// <summary>
/// Web-agnostic upload request — controller (task 20) extracts <c>file.OpenReadStream()</c>,
/// <c>file.ContentType</c>, <c>file.Length</c> from <c>IFormFile</c> before dispatching.
/// </summary>
public record UploadLotImageRequest(
    Guid LotId,
    Stream Content,
    string ContentType,
    long Length) : IRequest<Result<LotImageUploadModel>>;
