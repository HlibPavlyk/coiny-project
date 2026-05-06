using Coiny.Application.Abstractions.Email;
using Coiny.Infrastructure.ExternalServices.Resend;
using Coiny.Infrastructure.Identity;
using Coiny.Infrastructure.Jobs;
using Coiny.Infrastructure.Persistence.Extensions;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Resend;

namespace Coiny.Infrastructure.Extensions;

public static class DependencyContainerExtension
{
    public static void AddInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDatabase(configuration);
        services.AddIdentityInfrastructure(configuration);
        services.AddResendEmail(configuration);
        services.AddHangfireInfrastructure(configuration);
    }

    private static void AddResendEmail(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<ResendOptions>()
            .Bind(configuration.GetSection(ResendOptions.Section))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<ResendOptions>, ResendOptionsValidator>();

        services.AddOptions();
        services.AddHttpClient<ResendClient>();
        services.Configure<ResendClientOptions>(opts =>
            opts.ApiToken = configuration[$"{ResendOptions.Section}:ApiKey"] ?? string.Empty);
        services.AddTransient<IResend, ResendClient>();

        services.AddScoped<IEmailSender, ResendEmailSender>();
    }

    private static void AddHangfireInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        string connStr = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

        services.AddHangfire(cfg => cfg
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(opts => opts.UseNpgsqlConnection(connStr)));

        services.AddHangfireServer(opts =>
            opts.WorkerCount = Math.Min(Environment.ProcessorCount, 4));

        services.AddScoped<EmailOutboxFlushJob>();
    }
}
