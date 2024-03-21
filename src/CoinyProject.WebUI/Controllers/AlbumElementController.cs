using CoinyProject.Application.AlbumServices.Interfaces;
using CoinyProject.Application.AlbumServices.Services;
using CoinyProject.Application.DTO;
using CoinyProject.Core.Domain.Entities;
using CoinyProject.Infrastructure.Data;
using CoinyProject.Infrastructure.Data.Repositories;
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
     

    }
}
