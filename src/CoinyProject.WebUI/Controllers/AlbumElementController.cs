using CoinyProject.Application.AlbumServices.Interfaces;
using CoinyProject.Application.AlbumServices.Services;
using CoinyProject.Application.DTO;
using CoinyProject.Core.Domain.Entities;
using CoinyProject.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using System.Xml.Linq;
using static Azure.Core.HttpHeader;

namespace CoinyProject.WebUI.Controllers
{
    public class AlbumElementController : Controller
    {
        private readonly IAlbumService _albumService;
        private readonly IStringLocalizer<AlbumElementController> _localizer;

        public AlbumElementController(IAlbumService albumService, IStringLocalizer<AlbumElementController> localizer)
        {
            _albumService = albumService;
            _localizer = localizer;
        }

        public IActionResult Create(int id)
        {
            TempData["AlbumId"] = id.ToString();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(AlbumElementCreating element)
        {
            await _albumService.AddAlbumElement(element);
            TempData["success"] = Convert.ToString(_localizer["Album element successfully created"]);

            return RedirectToAction("Create");
        }

        public async Task<ActionResult> Edit(int id)
        {
            var album = await _albumService.GetAlbumElementForEdit(id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            return View(album);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(AlbumElementEditDTO album)
        {
            var id = await _albumService.UpdateAlbumElement(album);

            TempData["success"] = Convert.ToString(_localizer["Album element successfully updated"]);
            return RedirectToAction("Get","Album", new { id });
        }

        public async Task<ActionResult> Delete(int id)
        {
            var albumId = await _albumService.DeleteAlbumElement(id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            TempData["success"] = Convert.ToString(_localizer["Album element successfully deleted"]);
            return RedirectToAction("Get", "Album", new { id = albumId });
        }


    }
}
