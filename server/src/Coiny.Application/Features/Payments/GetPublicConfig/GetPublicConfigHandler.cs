using Coiny.Application.Abstractions.ExternalServices.Payments;
using Coiny.Application.Common.Results;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Coiny.Application.Features.Payments.GetPublicConfig;

public class GetPublicConfigHandler(
    IStripeClient stripe,
    IConfiguration configuration)
    : IRequestHandler<GetPublicConfigRequest, Result<PublicConfigModel>>
{
    public Task<Result<PublicConfigModel>> Handle(GetPublicConfigRequest request, CancellationToken ct)
    {
        string signalRHubUrl = configuration["Frontend:SignalRHubUrl"]
            ?? "/auctionHub";

        var model = new PublicConfigModel(
            StripePublishableKey: stripe.PublishableKey,
            SignalRHubUrl: signalRHubUrl,
            UahPerUsdDisplay: stripe.UahPerUsdRate);

        return Task.FromResult(Result.Success(model));
    }
}
