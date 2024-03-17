using CoinyProject.Application.AlbumServices.Interfaces;
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
        private readonly IAlbumService _albumService;

        public AlbumElementController(IAlbumService albumService)
        {
            _albumService = albumService;
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(AlbumElementCreating element)
        {
            await _albumService.AddAlbumElement(element);
            return  RedirectToAction("Create");
        }
        public async Task<IActionResult> Commit()
        {
            var (status, message) = await _albumService.CommitAlbumCreation();
            TempData[status] = message;

            if (status == "success")
                return RedirectToAction("Index","Album");
            else
                return RedirectToAction("Create");
        }

    }
}
