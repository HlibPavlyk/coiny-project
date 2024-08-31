using CoinyProject.Core.Domain.Entities;
using CoinyProject.IdentityServer;
using CoinyProject.Infrastructure.Data;
using CoinyProject.Shared.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDBConnection(builder.Configuration);
builder.Services.ConfigurateIdentityOptions();
builder.Services.AddIdentityUser();


builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "CoinyProject.IdentityServer.Cookie";
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
}); 


var migrationsAssembly = typeof(Program).Assembly.GetName().Name;

builder.Services.AddIdentityServer()
    .AddAspNetIdentity<User>()
    /*.AddConfigurationStore(options =>
    {
        options.ConfigureDbContext = b =>
            b.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(migrationsAssembly));
    })
    .AddOperationalStore(options =>
    {
        options.ConfigureDbContext = b =>
            b.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(migrationsAssembly));
    })*/
    .AddInMemoryApiResources(Configuration.GetApis())
    .AddInMemoryClients(Configuration.GetClients())
    .AddInMemoryIdentityResources(Configuration.GetIdentityResources())
    .AddDeveloperSigningCredential();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using var scope = app.Services.CreateScope();
{
    var userManager = scope.ServiceProvider
        .GetRequiredService<UserManager<User>>();
}

app.UseRouting();
app.UseIdentityServer();
app.UseAuthentication();


app.UseEndpoints(endpoints =>
{
    endpoints.MapDefaultControllerRoute();
});

app.Run();
