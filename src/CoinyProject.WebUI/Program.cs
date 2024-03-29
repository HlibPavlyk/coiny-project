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

var builder = WebApplication.CreateBuilder(args);

/*builder.Services.AddDbContext<CoinyProjectWebUIContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<CoinyProjectWebUIContext>();
*/

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);

builder.Services.AddRazorPages()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en-US"),
        new CultureInfo("uk-UA")
    };

    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

builder.Services.AddMvc().AddDataAnnotationsLocalization(options =>
{
    options.DataAnnotationLocalizerProvider = (type, factory) =>
    {
        var assemblyName = new AssemblyName(typeof(CoinyProject.Application.DTO.AlbumCreating).GetTypeInfo().Assembly.FullName);
        return factory.Create("Translations", assemblyName.Name);
    };

});

builder.Services.AddRazorPages();

builder.Services.AddDBConnection(builder.Configuration);
builder.Services.ConfigurateIdentityOptions();
builder.Services.AddIdentityUser();
builder.Services.AddScoped<IAlbumService, AlbumService>();

builder.Services.AddControllersWithViews().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    o.JsonSerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddAutoMapperService();



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

app.Run();
