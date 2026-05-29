using System.Text.Json.Serialization;
using Coiny.Api.Pipeline;
using Coiny.Api.Realtime;
using Coiny.Api.OpenApi;
using Coiny.Api.Services;
using Coiny.Application;
using Coiny.Application.Abstractions.Presentation.Http;
using Coiny.Application.Abstractions.Presentation.Realtime;
using Coiny.Application.Abstractions.ExternalServices.Search;
using Coiny.Application.Common.Json;
using Coiny.Application.Features.Demo;
using Coiny.Infrastructure;
using Coiny.Infrastructure.Jobs;
using Hangfire;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddMemoryCache();

// Demo-mode flag — gates the /api/v1/demo/* endpoint surface. Default Enabled = false; production
// returns 404 from those endpoints even to authenticated admins. See DemoModeOptions docs.
builder.Services.AddOptions<DemoModeOptions>()
    .Bind(builder.Configuration.GetSection(DemoModeOptions.Section));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, HttpContextCurrentUserService>();
builder.Services.AddScoped<IIpAddressResolver, HttpContextIpAddressResolver>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services
    .AddControllers(opts => opts.Filters.Add<ResultTransformFilter>())
    .AddJsonOptions(opts => ApplyJsonDefaults(opts.JsonSerializerOptions));

// Mirrors AddJsonOptions for OpenAPI schema generation — .NET reads schemas from Http.Json options.
builder.Services.ConfigureHttpJsonOptions(opts => ApplyJsonDefaults(opts.SerializerOptions));

static void ApplyJsonDefaults(System.Text.Json.JsonSerializerOptions opts)
{
    opts.PropertyNamingPolicy = JsonDefaults.Options.PropertyNamingPolicy;
    opts.PropertyNameCaseInsensitive = JsonDefaults.Options.PropertyNameCaseInsensitive;
    opts.DefaultIgnoreCondition = JsonDefaults.Options.DefaultIgnoreCondition;
    opts.Converters.Add(new JsonStringEnumConverter());
}

builder.Services.AddOpenApi("v1", opts =>
{
    opts.AddSchemaTransformer<UnwrapResultSchemaTransformer>();
    opts.AddOperationTransformer<ResultResponsesOperationTransformer>();
    opts.AddDocumentTransformer<JwtBearerSecuritySchemeTransformer>();
});

builder.Services.AddSignalR(options =>
{
    // Surface hub exception messages to clients in development; mask in production.
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});
builder.Services.AddScoped<IAuctionNotifier, SignalRAuctionNotifier>();

var app = builder.Build();

app.UseExceptionHandler();
app.MapOpenApi();
app.MapScalarApiReference(opts =>
{
    opts.Layout = ScalarLayout.Classic;
    opts.DefaultOpenAllTags = false;
});

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
    app.UseHangfireDashboard();

RecurringJob.AddOrUpdate<EmailOutboxFlushJob>(
    "email-outbox-flush",
    job => job.RunAsync(CancellationToken.None),
    Cron.Minutely());

// Search-index sync. 15s cron (6-field, with seconds) matching Hangfire's default 15s
// SchedulePollingInterval — declaring 10s would be aspirational since the poller caps it at 15s.
// THESIS-SCOPE §11 names 10s; 15s is the honest effective cadence (near-real-time for search).
RecurringJob.AddOrUpdate<SearchIndexFlushJob>(
    "search-index-flush",
    job => job.RunAsync(CancellationToken.None),
    "*/15 * * * * *");

RecurringJob.AddOrUpdate<RetryFailedWebhookJob>(
    "stripe-webhook-retry",
    job => job.RunAsync(CancellationToken.None),
    Cron.Hourly());

// Thesis demo cadence: poll every minute (the natural-cron sub-2-minute granularity) instead of
// the 15-min production cadence. Lets `Force → Delivered` / `Force → Returned` demo actions show
// the downstream Stripe webhook + UI update within seconds instead of forcing the demonstrator
// to wait for the next quarter-hour tick. Switch back to "*/15 * * * *" for production.
RecurringJob.AddOrUpdate<NovaPoshtaPollingJob>(
    "np-polling",
    job => job.RunAsync(CancellationToken.None),
    "* * * * *");

RecurringJob.AddOrUpdate<NonPaymentCancelJob>(
    "non-payment-cancel",
    job => job.RunAsync(CancellationToken.None),
    Cron.Daily());

RecurringJob.AddOrUpdate<PaymentReminderJob>(
    "payment-reminder",
    job => job.RunAsync(CancellationToken.None),
    Cron.Hourly());

// Ensure the Meilisearch index exists and its attribute config is applied before serving traffic.
// Idempotent — safe on every startup.
using (IServiceScope scope = app.Services.CreateScope())
{
    ISearchIndex searchIndex = scope.ServiceProvider.GetRequiredService<ISearchIndex>();
    await searchIndex.EnsureIndexAsync(CancellationToken.None);
}

app.MapControllers();
app.MapHub<AuctionHub>("/auctionHub");
app.Run();
