using Coiny.Application.Abstractions.ExternalServices.Payments;
using Coiny.Application.Common.Results;
using Coiny.Application.Features.Demo;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Coiny.Application.Features.Payments.GetPublicConfig;

public class GetPublicConfigHandler(
    IStripeClient stripe,
    IConfiguration configuration,
    IOptions<DemoModeOptions> demoMode)
    : IRequestHandler<GetPublicConfigRequest, Result<PublicConfigModel>>
{
    public Task<Result<PublicConfigModel>> Handle(GetPublicConfigRequest request, CancellationToken ct)
    {
        string signalRHubUrl = configuration["Frontend:SignalRHubUrl"]
            ?? "/auctionHub";

        var model = new PublicConfigModel(
            StripePublishableKey: stripe.PublishableKey,
            SignalRHubUrl: signalRHubUrl,
            UahPerUsdDisplay: stripe.UahPerUsdRate,
            DemoModeEnabled: demoMode.Value.Enabled);

        return Task.FromResult(Result.Success(model));
    }
}
