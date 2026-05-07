using Coiny.Application.Abstractions.Data;
using Coiny.Application.Abstractions.Providers;
using Coiny.Infrastructure.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Coiny.Infrastructure.Persistence.Extensions;

public static class DbConnectionExtensions
{
    public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default is not configured.");

        services.AddDbContext<ApplicationDbContext>(opts =>
            opts.UseNpgsql(connectionString, npg =>
                npg.MigrationsAssembly("Coiny.Infrastructure")));

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
    }
}
