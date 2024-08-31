using CoinyProject.Application.AlbumServices.Interfaces;
using CoinyProject.Application.DTO.Discussion;
using CoinyProject.Core.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace CoinyProject.WebUI.Controllers
{
    public class DiscussionController : Controller
    {
        private readonly IDiscussionService _discussionService;
        private readonly UserManager<User> _userManager;
        private readonly IStringLocalizer<DiscussionController> _localizer;
        public DiscussionController(IDiscussionService discussionService, UserManager<User> userManager,
            IStringLocalizer<DiscussionController> localizer)
        {
            _discussionService = discussionService;
            _userManager = userManager;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var discussions = await _discussionService.GetAllDiscussionsForView();
                return View(discussions);
            }
            catch
            {
                TempData["error"] = _localizer["An error occurred while loading discussions"];
                return RedirectToAction("Index", "Home");
            }
            
        }

        [Authorize]
        public async Task<IActionResult> Create()
        {
            try
            {
                ViewBag.AvailableTopics = await _discussionService.GetAvailableTopics();
                return View();
            }
            catch
            {
                TempData["error"] = _localizer["An error occurred while loading topics"];
                return RedirectToAction("Index");
            }
            
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> Create(DiscussionCreateDTO? discussion)
        {
            try
            {
                await _discussionService.AddDiscussion(discussion, _userManager.GetUserId(User));

                TempData["success"] = "Discussion successfully created";
                return RedirectToAction("Index", "Discussion");
            }
            catch
            {
                TempData["error"] = _localizer["An error occurred while creating discussion"];
                return RedirectToAction("Index");
            }
        }

        [Authorize]
        public async Task<IActionResult> Get(int? id)
        {
            try
            {
                var discussin = await _discussionService.GetDiscussionById(id);
                ViewBag.UserId = _userManager.GetUserId(User);

                return View(discussin);
            }
            catch
            {
                TempData["error"] = _localizer["An error occurred while loading discussion"];
                return RedirectToAction("Index");
            }
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> Get([FromBody] DiscussionMessageCreateDTO? dataToSend)
        {
            try
            {
                await _discussionService.AddDiscussionMessage(dataToSend);
                return Ok();
            }
            catch
            {
                TempData["error"] = _localizer["An error occurred while sending message"];
                return RedirectToAction("Index");
            }
            
        }
        
    }
}
