using AutoMapper;
using CoinyProject.Application.AlbumServices;
using CoinyProject.Application.AlbumServices.Interfaces;
using CoinyProject.Application.AlbumServices.Services;
using CoinyProject.Application.DTO.Album;
using CoinyProject.Core.Domain.Entities;
using CoinyProject.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using System.Xml.Linq;

namespace CoinyProject.WebUI.Controllers
{
    [Authorize]
    public class AlbumController : Controller
    {
        private readonly IAlbumService _albumService;
        private readonly UserManager<User> _userManager;
        private readonly IStringLocalizer<AlbumController> _localizer;

        public AlbumController(IAlbumService albumService, UserManager<User> userManager,
            IStringLocalizer<AlbumController> localizer)
        {
            _albumService = albumService;
            _userManager = userManager;
            _localizer = localizer;
        }

        public async Task<ActionResult> Index()
        {
            try
            {
                var albums = await _albumService.GetAllAlbumsDTO(_userManager.GetUserId(User));
                return View(albums);
            }
            catch
            {
                TempData["error"] = Convert.ToString(_localizer["Error getting albums"]);
                return RedirectToAction("Index", "Home");
            }

        }

        [ActionName("Get")]
        public async Task<ActionResult> GetAlbum(int? id)
        {
            try
            {
                var album = await _albumService.GetAlbumById(id);
                return View(album);
            }
            catch
            {
                TempData["error"] = Convert.ToString(_localizer["Error getting album"]);
                return RedirectToAction("Index");
            }
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(AlbumCreating? album)
        {
            try
            {
                var albumId = await _albumService.AddAlbum(album, _userManager.GetUserId(User));
                TempData["success"] = Convert.ToString(_localizer["Album successfully created"]);
                return RedirectToAction("Create", "AlbumElement", new { id = albumId });
            }
            catch
            {
                TempData["error"] = Convert.ToString(_localizer["Error creating album"]);
                return RedirectToAction("Create", "Album");
            }
        }

        public async Task<ActionResult> Edit(int? id)
        {
            try
            {
                var album = await _albumService.GetAlbumForEdit(id, _userManager.GetUserId(User));
                return View(album);
            }
            catch
            {
                TempData["error"] = Convert.ToString(_localizer["Error getting album"]);
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<ActionResult> Edit(AlbumEditDTO? album)
        {
            try
            {
                await _albumService.UpdateAlbum(album);
                TempData["success"] = Convert.ToString(_localizer["Album successfully updated"]);
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["error"] = Convert.ToString(_localizer["Error updating album"]);
                return RedirectToAction("Index");
            }
        }

        public async Task<ActionResult> Delete(int? id)
        {
            try
            {
                await _albumService.DeleteAlbum(id, _userManager.GetUserId(User));
                TempData["success"] = Convert.ToString(_localizer["Album successfully deleted"]);
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["error"] = Convert.ToString(_localizer["Error deleting album"]);
                return RedirectToAction("Index");
            }
        }

    }
}
