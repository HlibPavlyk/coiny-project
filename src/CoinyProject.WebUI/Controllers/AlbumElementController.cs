﻿using CoinyProject.Application.AlbumServices.Interfaces;
using CoinyProject.Application.AlbumServices.Services;
using CoinyProject.Application.DTO.Album;
using CoinyProject.Core.Domain.Entities;
using CoinyProject.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<User> _userManager;

        public AlbumElementController(IAlbumService albumService, 
            IStringLocalizer<AlbumElementController> localizer,
            UserManager<User> userManager)
        {
            _albumService = albumService;
            _localizer = localizer;
            _userManager = userManager;
        }

        public IActionResult Create(int? id)
        {
            TempData["AlbumId"] = id.ToString();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(AlbumElementCreating element)
        {
            try
            {
                await _albumService.AddAlbumElement(element);
                TempData["success"] = Convert.ToString(_localizer["Album element successfully created"]);

                return RedirectToAction("Create");
            }
            catch
            {
                TempData["error"] = Convert.ToString(_localizer["Error creating album element"]);
                return RedirectToAction("Create");
            }
            
        }

        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                var album = await _albumService.GetAlbumElementForEdit(id, _userManager.GetUserId(User));
                return View(album);
            }
            catch
            {
                TempData["error"] = Convert.ToString(_localizer["Error getting album element"]);
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<ActionResult> Edit(AlbumElementEditDTO album)
        {
            try
            {
                var id = await _albumService.UpdateAlbumElement(album);

                TempData["success"] = Convert.ToString(_localizer["Album element successfully updated"]);
                return RedirectToAction("Get", "Album", new { id });
            }
            catch
            {
                TempData["error"] = Convert.ToString(_localizer["Error updating album element"]);
                return RedirectToAction("Index", "Home");
            }
            
        }

        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var albumId = await _albumService.DeleteAlbumElement(id, _userManager.GetUserId(User));

                TempData["success"] = Convert.ToString(_localizer["Album element successfully deleted"]);
                return RedirectToAction("Get", "Album", new { id = albumId });
            }
            catch
            {
                TempData["error"] = Convert.ToString(_localizer["Error getting album element"]);
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
