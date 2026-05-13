using Amazon.Runtime;
using Amazon.S3;
using Coiny.Application.Abstractions.Email;
using Coiny.Application.Abstractions.Files;
using Coiny.Application.Abstractions.Jobs;
using Coiny.Application.Abstractions.Payments;
using Coiny.Application.Abstractions.Shipping;
using Coiny.Infrastructure.ExternalServices.NovaPoshta;
using Coiny.Infrastructure.ExternalServices.Resend;
using Coiny.Infrastructure.ExternalServices.Stripe;
using Coiny.Infrastructure.Files;
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
        services.AddR2FileStorage(configuration);
        services.AddStripe(configuration);
        services.AddNovaPoshta(configuration);
    }

    private static void AddNovaPoshta(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<NovaPoshtaOptions>()
            .Bind(configuration.GetSection(NovaPoshtaOptions.Section))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<NovaPoshtaOptions>, NovaPoshtaOptionsValidator>();

        // The real client owns its HttpClient (and gets a standard resilience pipeline:
        // retry on 5xx/408/429 + timeout + circuit breaker). HybridNovaPoshtaClient
        // delegates read methods to this real client.
        services.AddHttpClient<NovaPoshtaClient>()
            .AddStandardResilienceHandler();

        // Default for the thesis build: hybrid (real reads, synthetic TTNs + simulated polling).
        // Swap to NovaPoshtaClient explicitly when actual parcel creation is desired.
        services.AddScoped<INovaPoshtaClient, HybridNovaPoshtaClient>();
    }

    private static void AddStripe(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<StripeOptions>()
            .Bind(configuration.GetSection(StripeOptions.Section))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<StripeOptions>, StripeOptionsValidator>();

        services.AddSingleton<IStripeClient, StripeClient>();

        services.AddScoped<StripeWebhookProcessor>();
        services.AddScoped<RetryFailedWebhookJob>();
    }

    private static void AddR2FileStorage(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<R2Options>()
            .Bind(configuration.GetSection(R2Options.Section))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<R2Options>, R2OptionsValidator>();

        services.AddSingleton<IAmazonS3>(sp =>
        {
            R2Options opts = sp.GetRequiredService<IOptions<R2Options>>().Value;
            var credentials = new BasicAWSCredentials(opts.AccessKeyId, opts.SecretAccessKey);
            var config = new AmazonS3Config
            {
                ServiceURL = $"https://{opts.AccountId}.r2.cloudflarestorage.com",
                ForcePathStyle = true,
                // Cloudflare R2 doesn't support the new "streaming SigV4 + trailing checksum" mode
                // that AWSSDK.S3 3.7.400+ uses by default. Disable it.
                RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
                ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED,
            };
            return new AmazonS3Client(credentials, config);
        });

        services.AddScoped<IFileService, R2FileService>();
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
        services.AddScoped<IAuctionCloseJob, AuctionCloseJob>();
        services.AddScoped<ICreateTtnJob, CreateTtnJob>();
        services.AddScoped<NovaPoshtaPollingJob>();
        services.AddScoped<IJobScheduler, HangfireJobScheduler>();
    }
}
