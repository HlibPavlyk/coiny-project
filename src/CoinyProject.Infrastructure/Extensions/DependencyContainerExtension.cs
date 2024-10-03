using CoinyProject.Application.Abstractions.DataServices;
using CoinyProject.Application.Abstractions.Repositories;
using CoinyProject.Application.Abstractions.Services;
using CoinyProject.Application.AutoMapper;
using CoinyProject.Application.AutoMapper.Resolvers;
using CoinyProject.Application.Services;
using CoinyProject.Infrastructure.DataService;
using CoinyProject.Infrastructure.Repositories;
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
        services.AddTransient<GetISeveralImageUrlsResolver>();
        services.AddAutoMapperService();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAlbumService, AlbumService>();
        services.AddScoped<IAlbumElementService, AlbumElementService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserService, UserService>();

        services.AddIdentityUser();
        services.ConfigurateIdentityOptions();
        services.AddJwtAuthentication(configuration);
    }
}