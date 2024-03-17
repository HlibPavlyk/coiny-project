using CoinyProject.Infrastructure.Data;
using CoinyProject.IdentityServer.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using CoinyProject.Core.Domain.Entities;
using CoinyProject.WebUI.Areas.Identity;
using CoinyProject.Application.AlbumServices.Interfaces;
using CoinyProject.Application.AlbumServices.Services;

var builder = WebApplication.CreateBuilder(args);

/*builder.Services.AddDbContext<CoinyProjectWebUIContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<CoinyProjectWebUIContext>();
*/

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDBConnection(builder.Configuration);
builder.Services.ConfigurateIdentityOptions();
builder.Services.AddIdentityUser();
builder.Services.AddScoped<IAlbumService, AlbumService>();

var app = builder.Build();

app.DBEnsureCreated();

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

app.Run();
