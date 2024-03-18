using CoinyProject.Application.AlbumServices;
using CoinyProject.Application.AlbumServices.Interfaces;
using CoinyProject.Application.AlbumServices.Services;
using CoinyProject.Application.DTO;
using CoinyProject.Infrastructure.Data;
using CoinyProject.Infrastructure.Data.Repositories;
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

        [Route("album-details/{id:int:min(1)}", Name = "albumDetailsRoute"), ActionName("Get")]
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
            _albumService.AddAlbum(album);
            return RedirectToAction("Create","AlbumElement");
        }

    }
}
