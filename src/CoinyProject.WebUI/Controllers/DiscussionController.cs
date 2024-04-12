using CoinyProject.Application.AlbumServices.Interfaces;
using CoinyProject.Application.DTO.Discussion;
using CoinyProject.Core.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CoinyProject.WebUI.Controllers
{
    [Authorize]
    public class DiscussionController : Controller
    {
        private readonly IDiscussionService _discussionService;
        private readonly UserManager<User> _userManager;
        public DiscussionController(IDiscussionService discussionService, UserManager<User> userManager)
        {
            _discussionService = discussionService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var discussions = await _discussionService.GetAllDiscussionsForView();
            return View(discussions);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.AvailableTopics = await _discussionService.GetAvailableTopics();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(DiscussionCreateDTO discussion)
        {
            await _discussionService.AddDiscussion(discussion, _userManager.GetUserId(User));

            TempData["success"] = "Discussion successfully created";
            return RedirectToAction("Index", "Discussion");

        }

        public async Task<IActionResult> Get(int id)
        {
            if (id != 0)
            {
                var discussin = await _discussionService.GetDiscussionById(id);
                ViewBag.UserId = _userManager.GetUserId(User);

                return View(discussin);
            }
            return NotFound();
            
        }
        [HttpPost]
        public async Task<IActionResult> Get([FromBody] DiscussionMessageCreateDTO dataToSend)
        {
            if (ModelState.IsValid)
            {
                await _discussionService.AddDiscussionMessage(dataToSend);
                return Ok();
            }
            return BadRequest();
        }
        
    }
}
