using CoinyProject.Infrastructure.Data;
using CoinyProject.IdentityServer.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using CoinyProject.Core.Domain.Entities;
using CoinyProject.Application.AlbumServices.Interfaces;
using CoinyProject.Application.AlbumServices.Services;
using System.Drawing.Text;
using CoinyProject.Application.AutoMapper;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using System.Reflection;
using CoinyProject.WebUI.Extensions;
using CoinyProject.WebUI.Hubs;
using CoinyProject.UnitTests.Shared;
using CoinyProject.Infrastructure.Data.Repositories.Realization;
using CoinyProject.Infrastructure.Data.Repositories.Interfaces;
using CoinyProject.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCustomLocalization();

builder.Services.AddAuthentication(config =>
{
    config.DefaultScheme = "Cookie";
    config.DefaultChallengeScheme = "oidc";

})
    .AddCookie("Cookie")
    .AddOpenIdConnect("oidc", config =>
    {
        config.Authority = "https://localhost:5443";
        config.ClientId = "client_id_ui";
        config.ClientSecret = "client_secret_ui";
        config.SaveTokens = true;
        config.ResponseType = "code";
        config.SignedOutCallbackPath = "/Home/Index";
        config.GetClaimsFromUserInfoEndpoint = true;


    });

builder.Services.AddRazorPages();

builder.Services.AddDBConnection(builder.Configuration);
builder.Services.ConfigurateIdentityOptions();
builder.Services.AddIdentityUser();
builder.Services.AddTransient<FakeDataGenerator>();

builder.Services.AddScoped<IAlbumService, AlbumService>(); 
builder.Services.AddScoped<IDiscussionService, DiscussionService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddControllersWithViews().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    o.JsonSerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddAutoMapperService();

builder.Services.AddSignalR();

var app = builder.Build();

app.UseRequestLocalization();

/*app.DBEnsureCreated();*/

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<DiscussionHub>("/chat");


app.Run();
