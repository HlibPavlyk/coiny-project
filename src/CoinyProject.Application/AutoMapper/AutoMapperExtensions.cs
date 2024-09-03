using Microsoft.Extensions.DependencyInjection;

namespace CoinyProject.Application.AutoMapper
{
    public static class AutoMapperExtensions
    {
        public static void AddAutoMapperService(this IServiceCollection service)
        {
            IServiceCollection serviceCollection = service.AddAutoMapper(typeof(AutoMapperProfile).Assembly);

        }
    }
}
