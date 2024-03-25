using AutoMapper;
using CoinyProject.Application.AlbumServices;
using CoinyProject.Application.AlbumServices.Interfaces;
using CoinyProject.Application.AlbumServices.Services;
using CoinyProject.Application.DTO;
using CoinyProject.Core.Domain.Entities;
using CoinyProject.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace CoinyProject.WebUI.Controllers
{
    public class AlbumController : Controller
    {
        private readonly IAlbumService _albumService;

        public AlbumController(IAlbumService albumService)
        {
            _albumService = albumService;
        }

        public async Task<ActionResult> Index()
        {
            var albums = await _albumService.GetAllAlbumsDTO();
            return View(albums);
        }

        [ActionName("Get")]
        public async Task<ActionResult> GetAlbum(int id)
        {
            var album = await _albumService.GetAlbumById(id);
            return View(album);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(AlbumCreating album)
        {
            var albumId = await _albumService.AddAlbum(album);
            return RedirectToAction("Create", "AlbumElement", new { id = albumId });
        }

        public async Task<ActionResult> Edit(int id)
        {
            var album = await _albumService.GetAlbumForEdit(id);
            return View(album);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(AlbumEditDTO album)
        {
            await _albumService.UpdateAlbum(album);
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Delete(int id)
        {
            await _albumService.DeleteAlbum(id);
            return RedirectToAction("Index");
        }

    }
}
