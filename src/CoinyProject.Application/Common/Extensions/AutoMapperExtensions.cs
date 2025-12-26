using CoinyProject.Application.Common.AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace CoinyProject.Application.Common.Extensions
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
