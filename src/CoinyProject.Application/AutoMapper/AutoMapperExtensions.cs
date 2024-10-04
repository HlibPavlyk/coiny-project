using Microsoft.Extensions.DependencyInjection;

namespace CoinyProject.Application.AutoMapper
{
    public static class AutoMapperExtensions
    {
        public static void AddAutoMapperService(this IServiceCollection service)
        {
            service.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<AutoMapperProfile>();
            });
        }
    }
}
