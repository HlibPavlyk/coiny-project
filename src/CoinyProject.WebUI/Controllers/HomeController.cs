using CoinyProject.Application.AlbumServices.Interfaces;
using CoinyProject.Core.Domain.Entities;
using CoinyProject.WebUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace CoinyProject.WebUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAlbumService _albumService;
        private readonly UserManager<User> _userManager;
        public HomeController(IAlbumService albumService, UserManager<User> userManager)
        {
            _albumService = albumService;
            _userManager = userManager;
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
            var albums = await _albumService.GetAllAlbumsForView(_userManager.GetUserId(User));
            return View(albums);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Index(int id)
        {
            await _albumService.LikeAlbum(id, _userManager.GetUserId(User));
            return RedirectToAction("Index");
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
