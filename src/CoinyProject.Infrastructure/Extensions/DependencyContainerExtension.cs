using CoinyProject.Application;
using CoinyProject.Application.Abstractions.Data;
using CoinyProject.Application.Abstractions.Identity;
using CoinyProject.Application.Common.AutoMapper.Resolvers;
using CoinyProject.Application.Common.Extensions;
using CoinyProject.Infrastructure.Services.Data;
using CoinyProject.Infrastructure.Services.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoinyProject.Infrastructure.Extensions;

public static class DependencyContainerExtension
{
    public static void AddDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbEfConnection(configuration);
        services.AddHttpContextAccessor();

        services.AddScoped<IFileService, FileService>();
        services.AddTransient<GetImageUrlResolver>();
        //services.AddTransient<GetISeveralImageUrlsResolver>();
        services.AddAutoMapperService();

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        // Identity services
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ITokenGenerator, JwtTokenGenerator>();

        // register mediator handlers
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
            typeof(ApplicationAssemblyType).Assembly
        ));

        services.AddIdentityUser();
        services.ConfigurateIdentityOptions();
        services.AddJwtAuthentication(configuration);
    }
}