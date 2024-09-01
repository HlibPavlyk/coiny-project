using CoinyProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Infrastructure
{
    public static class ConnectionExtensions
    {
        public static void AddDBConnection(this IServiceCollection service, IConfiguration configuration)
        {
            IServiceCollection serviceCollection = service.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        }
    }


}
