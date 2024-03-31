using CoinyProject.Application.AlbumServices.Interfaces;
using CoinyProject.Core.Domain.Entities;
using CoinyProject.WebUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CoinyProject.WebUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAlbumService _albumService;
        public HomeController(IAlbumService albumService)
        {
            _albumService = albumService;
        }
        
        [HttpPost]
        public IActionResult CultureManager(string culture)
        {
            Response.Cookies.Append(
                  CookieRequestCultureProvider.DefaultCookieName,
                  CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                  new CookieOptions { Expires = DateTimeOffset.Now.AddDays(30) }
              );

            return Redirect(Request.Headers["Referer"].ToString());
        }

        public async Task<ActionResult> Index()
        {
            var albums = await _albumService.GetAllAlbumsForView();
            return View(albums);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
