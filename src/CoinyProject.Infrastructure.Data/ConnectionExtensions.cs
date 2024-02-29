using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Infrastructure.Data
{
    public static class ConnectionExtensions
    {
        public static void AddDBConnection(this IServiceCollection service, IConfiguration configuration)
        {
            IServiceCollection serviceCollection = service.AddDbContext<ApplicationDBContext>(options =>
            
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                /*options.UseLazyLoadingProxies();
                options.Use*/

            );
        }
    }


}
