using CoinyProject.Application.AlbumServices.Services;
using CoinyProject.Application.DTO;
using CoinyProject.Core.Domain.Entities;
using CoinyProject.Infrastructure.Data;
using CoinyProject.Infrastructure.Data.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace CoinyProject.WebUI.Controllers
{
    public class AlbumElementController : Controller
    {
        private readonly AlbumElementService _albumElementService;

        public AlbumElementController(ApplicationDBContext db)
        {
            _albumElementService = new AlbumElementService(new UnitOfWork(db));
        }

        [HttpGet]
        public async Task<IActionResult> Create(int albumId)
        {
            await _albumElementService.SetAlbumId(albumId);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(AlbumElementCreating element)
        {
            await _albumElementService.AddAlbumElement(element);
            return  RedirectToAction("Create");
        }
        public async Task<IActionResult> Commit()
        {
            await _albumElementService.CommitAlbumElementList();
            return RedirectToAction("Index","Album");
        }

    }
}
