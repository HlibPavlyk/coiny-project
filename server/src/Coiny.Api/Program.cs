using Coiny.Api.Filters;
using Coiny.Api.Middleware;
using Coiny.Infrastructure.Persistence.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDatabase(builder.Configuration);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddControllers(opts => opts.Filters.Add<ResultTransformFilter>());

var app = builder.Build();

app.UseExceptionHandler();
app.MapControllers();
app.Run();
