using System.Text.Json;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Lots.Models;
using Coiny.Domain.Enums;
using MediatR;

namespace Coiny.Application.Features.Lots.Requests;

public record CreateLotRequest(
    string Title,
    string Description,
    int CategoryId,
    LotCondition Condition,
    long StartingPriceUahKopiykas,
    DateTime EndsAt,
    JsonElement Attributes) : IRequest<Result<LotCreatedModel>>;
