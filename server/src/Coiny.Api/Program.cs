using System.Text.Json.Serialization;
using Coiny.Api.Filters;
using Coiny.Api.Hubs;
using Coiny.Api.Middleware;
using Coiny.Api.OpenApi;
using Coiny.Api.Services;
using Coiny.Application.Abstractions.Http;
using Coiny.Application.Common.Json;
using Coiny.Application.Extensions;
using Coiny.Infrastructure.Extensions;
using Coiny.Infrastructure.Jobs;
using Hangfire;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddMemoryCache();

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

builder.Services.AddSignalR();

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

app.MapControllers();
app.MapHub<AuctionHub>("/auctionHub");
app.Run();
