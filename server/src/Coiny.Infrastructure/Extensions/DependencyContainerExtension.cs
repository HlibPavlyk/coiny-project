using Coiny.Infrastructure.Identity;
using Coiny.Infrastructure.Persistence.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Coiny.Infrastructure.Extensions;

public static class DependencyContainerExtension
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDatabase(configuration);
        services.AddIdentityInfrastructure(configuration);
        return services;
    }
}
