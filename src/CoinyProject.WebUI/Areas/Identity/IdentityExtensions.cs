using CoinyProject.Core.Domain.Entities;
using CoinyProject.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CoinyProject.WebUI.Areas.Identity
{
    public static class IdentityExtensions
    {
        public static void AddIdentityUser(this IServiceCollection service)
        {
            service.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<ApplicationDBContext>();
        }

        public static void ConfigurateIdentityOptions(this IServiceCollection service)
        {
            service.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = false;
            });
        }
    }
}
