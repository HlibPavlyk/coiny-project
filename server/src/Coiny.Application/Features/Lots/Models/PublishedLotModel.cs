using Coiny.Domain.Enums;

namespace Coiny.Application.Features.Lots.Models;

public record PublishedLotModel(Guid Id, LotStatus Status, DateTime StartsAt, DateTime EndsAt);
