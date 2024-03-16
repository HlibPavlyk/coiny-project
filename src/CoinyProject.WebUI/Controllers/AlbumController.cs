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

        public AlbumController(ApplicationDBContext db)
        {
            _albumService = new AlbumService(new UnitOfWork(db));
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(AlbumCreating album)
        {
            int albumId = await _albumService.AddAlbum(album);
            return RedirectToAction("Create","AlbumElement", new { albumId });
        }

    }
}
