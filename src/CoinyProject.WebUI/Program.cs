using CoinyProject.Infrastructure.Data;
using CoinyProject.IdentityServer.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using CoinyProject.Core.Domain.Entities;
using CoinyProject.WebUI.Areas.Identity;
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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCustomLocalization();

builder.Services.AddRazorPages();

builder.Services.AddDBConnection(builder.Configuration);
builder.Services.ConfigurateIdentityOptions();
builder.Services.AddIdentityUser();

builder.Services.AddScoped<IAlbumService, AlbumService>(); 
builder.Services.AddScoped<IDiscussionService, DiscussionService>();

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

app.UseAuthorization();

app.MapRazorPages();



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<DiscussionHub>("/chat");


app.Run();
