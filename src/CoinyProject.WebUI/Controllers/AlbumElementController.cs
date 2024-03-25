using CoinyProject.Application.AlbumServices.Interfaces;
using CoinyProject.Application.AlbumServices.Services;
using CoinyProject.Application.DTO;
using CoinyProject.Core.Domain.Entities;
using CoinyProject.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using static Azure.Core.HttpHeader;

namespace CoinyProject.WebUI.Controllers
{
    public class AlbumElementController : Controller
    {
        private readonly IAlbumService _albumService;

        public AlbumElementController(IAlbumService albumService)
        {
            _albumService = albumService;
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
            TempData["success"] = "Album element successfuly created";

            return RedirectToAction("Create");
        }

        public async Task<ActionResult> Edit(int id)
        {
            var album = await _albumService.GetAlbumElementForEdit(id);
            return View(album);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(AlbumElementEditDTO album)
        {
            var id = await _albumService.UpdateAlbumElement(album);
            TempData["success"] = "Album element successfuly updated";
            return RedirectToAction("Get","Album", new { id });
        }

        public async Task<ActionResult> Delete(int id)
        {
            var albumId = await _albumService.DeleteAlbumElement(id);
            TempData["success"] = "Album element successfuly deleted";
            return RedirectToAction("Get", "Album", new { id = albumId });
        }


    }
}
