using CoinyProject.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
