using Coiny.Domain.Enums;

namespace Coiny.Application.Features.Lots.PublishLot;

public record PublishedLotModel(Guid Id, LotStatus Status, DateTime StartsAt, DateTime EndsAt);
