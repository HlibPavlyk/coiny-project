using Coiny.Api.Filters;
using Coiny.Api.Middleware;
using Coiny.Api.OpenApi;
using Coiny.Infrastructure.Extensions;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddControllers(opts => opts.Filters.Add<ResultTransformFilter>());

builder.Services.AddOpenApi("v1", opts =>
{
    opts.AddSchemaTransformer<UnwrapResultSchemaTransformer>();
    opts.AddOperationTransformer<ResultResponsesOperationTransformer>();
    opts.AddDocumentTransformer<JwtBearerSecuritySchemeTransformer>();
});

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

app.MapControllers();
app.Run();
